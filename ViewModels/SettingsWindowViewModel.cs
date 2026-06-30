using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    readonly DataStorageService _dataStorageService;
    readonly WindowNotificationManager _notificationManager;

    [ObservableProperty]
    public partial string? SelectedStrategyNameDisplay {  get; set; }

    public ObservableCollection<string> StrategyNames { get; set; } = [];

    [ObservableProperty]
    public partial string? SelectedStrategyName { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateChackActive { get; set; } = true;

    [ObservableProperty]
    public partial GameFilterStatus GameFilterStatus { get; set; }


    public SettingsWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
        _notificationManager = App.NotificationManager;
        SelectedStrategyNameDisplay = "";
    }

    [RelayCommand]
    private async Task ScanAndSaveStrategies()
    {
        if (string.IsNullOrWhiteSpace(App.ZapretFolderPath))
            return;

        var strategies = FileParserService.LoadAllStrategies(App.ZapretFolderPath);
        await _dataStorageService.SaveStrategies(strategies);
        StrategyNames.Clear();
        foreach (var s in strategies)
        {
            StrategyNames.Add(s.Name);
            App.LoadedStrategies.Add(s);
        }
    }

    async partial void OnSelectedStrategyNameChanged(string? value)
    {
        if (value == null)
            return;

        _dataStorageService.ExternalLibraryResources.SelectedStrategyName = value;
        await _dataStorageService.SaveSelectedStrategyName(value);

        SelectedStrategyNameDisplay = value;
    }

    async partial void OnIsUpdateChackActiveChanged(bool value)
    {
        try
        {
            var path = Path.Combine(App.ZapretFolderPath, "utils\\check_updates.enabled");

            if (value)
            {
                if (File.Exists(path))
                    return;

                File.Create(path);
                return;
            }

            if (!File.Exists(path))
                return;

            File.Delete(path);
        }
        catch
        {

        }
    }

    async partial void OnGameFilterStatusChanged(GameFilterStatus value)
    {
        var filePath = Path.Combine(App.ZapretFolderPath, "utils\\game_filter.enabled");

        try
        {
            if (value == GameFilterStatus.Disabled && File.Exists(filePath))
            {
                File.Delete(filePath);
                return;
            }

            if (value == GameFilterStatus.All)
            {
                await File.WriteAllTextAsync(filePath, "all");
                return;
            }

            if (value == GameFilterStatus.TCP)
            {
                await File.WriteAllTextAsync(filePath, "tcp");
                return;
            }

            if (value == GameFilterStatus.UDP)
            {
                await File.WriteAllTextAsync(filePath, "udp");
                return;
            }
        }
        catch
        {
            _notificationManager.Show(new Notification()
            {
                Type = NotificationType.Error,
                Expiration = TimeSpan.FromSeconds(5),
                Message = "Ошибка записи фала с настройками"
            });
        }
    }

    public async Task OnViewLoaded()
    {
        StrategyNames.Clear();
        foreach (var strategy in _dataStorageService.ExternalLibraryResources.Strategies)
        {
            StrategyNames.Add(strategy.Name);
        }

        SelectedStrategyName = _dataStorageService.ExternalLibraryResources.SelectedStrategyName;

        try
        {
            var path = Path.Combine(App.ZapretFolderPath, "utils\\check_updates.enabled");
            IsUpdateChackActive = File.Exists(path);
        }
        catch 
        {
            _notificationManager.Show(new Notification()
            {
                Type = NotificationType.Error,
                Expiration = TimeSpan.FromSeconds(5),
                Message = "Ошибка чтения фала с настройками"
            });
        }

        GameFilterStatus = await CheckCurrentGameFilterStatus();
    }


    private async Task<GameFilterStatus> CheckCurrentGameFilterStatus()
    {
        var filePath = Path.Combine(App.ZapretFolderPath, "utils\\game_filter.enabled");

        try
        {
            if (!File.Exists(filePath))
                return GameFilterStatus.Disabled;

            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                ReadOnlySpan<char> wordSpan = line.AsSpan().Trim();

                if (wordSpan.Equals("all", StringComparison.OrdinalIgnoreCase))
                    return GameFilterStatus.All;

                if (wordSpan.Equals("tcp", StringComparison.OrdinalIgnoreCase))
                    return GameFilterStatus.TCP;
            }

            return GameFilterStatus.UDP;
        }
        catch 
        {
            _notificationManager.Show(new Notification()
            {
                Type = NotificationType.Error,
                Expiration = TimeSpan.FromSeconds(5),
                Message = "Ошибка чтения фала с настройками"
            });

            return GameFilterStatus.Disabled;
        }
    }
}

public enum GameFilterStatus
{
    Disabled = 0,
    All = 1,
    TCP = 2,
    UDP = 3,
}