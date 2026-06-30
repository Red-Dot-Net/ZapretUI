using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class DiagnosticsWindowViewModel : ViewModelBase
{
    private readonly DataStorageService _dataStorageService;
    private readonly WindowNotificationManager _notificationManager;

    [ObservableProperty]
    public partial string? BestStrategyName {  get; set; } = string.Empty;

    public DiagnosticsWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
        _notificationManager = App.NotificationManager;
    }

    public async Task OnLoadWindow()
    {
        BestStrategyName = _dataStorageService.ExternalLibraryResources?.BestStrategy;
    }

    [RelayCommand]
    private async Task ScanDiagnosticsFile()
    {
        var dirPath = Path.Combine(App.ZapretFolderPath, "utils\\test results");
        if (Directory.Exists(dirPath))
        {
            List<string> correctTypeFilePaths = [];
            var filePaths = Directory.GetFiles(dirPath);
            if (filePaths == null || filePaths.Length == 0)
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Диагностика еще ни разу не проводилась. Файл с результатами отсутствует."
                });
                return;
            }

            foreach (var filePath in filePaths)
            {
                if (await IsOfTypeStandart(filePath))
                {
                    correctTypeFilePaths.Add(filePath);
                }
            }

            if (correctTypeFilePaths.Count == 1)
            {
                var bestStrategy = await GetBestStrategyFromFile(correctTypeFilePaths[0]);
                if (bestStrategy == null)
                {
                    _notificationManager.Show(new Notification()
                    {
                        Expiration = TimeSpan.FromSeconds(5),
                        Type = NotificationType.Error,
                        Message = "Ошибка чтения файла диагностики."
                    });
                    return;
                }

                await _dataStorageService.SaveBestStrategy(bestStrategy);
                BestStrategyName = bestStrategy;

                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Message = "Успешно найдено",
                    Type = NotificationType.Success,
                });

                return;
            }

            int latestFileIndex = GetLatestFileIndex(correctTypeFilePaths);
            if (latestFileIndex < 0)
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Ошибка чтения файла диагностики."
                });
                return;
            }

            var bestStrategy2 = await GetBestStrategyFromFile(correctTypeFilePaths[latestFileIndex]);
            if (bestStrategy2 == null)
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Ошибка чтения файла диагностики."
                });
                return;
            }

            await _dataStorageService.SaveBestStrategy(bestStrategy2);
            BestStrategyName = bestStrategy2;
            return;
        }
    }

    [RelayCommand]
    private async Task BeginNewDiagnostic()
    {
        try
        {
            var scriptPath = Path.Combine(App.ZapretFolderPath, "utils\\test zapret.ps1");
            if (!File.Exists(scriptPath))
            {
                _notificationManager.Show(new Notification()
                    {
                    Expiration = TimeSpan.FromSeconds(5),    
                    Type = NotificationType.Error,
                        Message = "Скрипт диагностики не найден."
                });
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Normal
            };

            Process? process = Process.Start(startInfo);
            process?.Dispose();
        }
        catch
        {
            _notificationManager.Show(new Notification()
            {
                Expiration = TimeSpan.FromSeconds(5),
                Type = NotificationType.Error,
                Message = "Ошибка запуска файла диагностики."
            });
            return;
        }
    }



    private int GetLatestFileIndex(List<string> fileFullPaths)
    {
        string[] fileNames = new string[fileFullPaths.Count];
        for (int j = 0; j < fileFullPaths.Count; j++)
        {
            fileNames[j] = Path.GetFileNameWithoutExtension(fileFullPaths[j]);
        }

        int latestFileIndex = 0;
        DateOnly latestDate = default;
        TimeOnly latestTime = default;
        for (int i = 0; i < fileNames.Length; i++)
        {
            string[] nameStrings = fileNames[i].Split('_');
            if (nameStrings.Length < 4)
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Диагностика еще ни разу не проводилась. Файл с результатами отсутствует."
                });
                return -1;
            }

            if (!DateOnly.TryParseExact(nameStrings[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Ошибка определения даты проведения диагностики."
                });
                return -1;
            }

            if (!TimeOnly.TryParseExact(nameStrings[3], "HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Ошибка определения времени проведения диагностики."
                });
                return -1;
            }

            if (i == 0)
            {
                latestDate = date;
                latestTime = time;
                continue;
            }

            if (date > latestDate)
            {
                latestDate = date;
                latestTime = time;
                latestFileIndex = i;
            }
            else if (date == latestDate)
            {
                if (time > latestTime)
                {
                    latestTime = time;
                    latestFileIndex = i;
                }
            }
        }

        return latestFileIndex;
    }

    private async Task<string> GetBestStrategyFromFile(string filePath)
    {
        try
        {
            var lastLine = await File.ReadLinesAsync(filePath).LastAsync();
            var words = lastLine.Split(':');
            if (words.Length < 2)
            {
                _notificationManager.Show(new Notification()
                {
                    Expiration = TimeSpan.FromSeconds(5),
                    Type = NotificationType.Error,
                    Message = "Ошибка чтения файла диагностики."
                });
                return string.Empty;
            }

            return words[1].Trim();
        }
        catch 
        {
            _notificationManager.Show(new Notification()
            {
                Expiration = TimeSpan.FromSeconds(5),
                Type = NotificationType.Error,
                Message = "Ошибка чтения файла диагностики."
            });
            return string.Empty;
        }
    }

    private async Task<bool> IsOfTypeStandart(string filePath)
    {
        const string keyword = "Type:";
        try
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                int index = line.IndexOf(keyword, StringComparison.Ordinal);
                if (index < 0)
                    continue;

                ReadOnlySpan<char> typeSpan = line.AsSpan(index + keyword.Length).Trim();
                if (typeSpan.Length >= 4 && typeSpan.StartsWith("stan", StringComparison.Ordinal))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
