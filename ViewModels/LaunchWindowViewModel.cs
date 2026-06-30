using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class LaunchWindowViewModel : ViewModelBase
{
    readonly DataStorageService _dataStorageService;
    readonly WindowNotificationManager _notificationManager;
    static int _loadIndex = 0;

    [ObservableProperty]
    public partial string? SelectedStrategyName {  get; set; }

    public LaunchWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
        _notificationManager = App.NotificationManager;
        SelectedStrategyName = "";
    }

    [RelayCommand]
    private async Task Launch()
    {
        if (string.IsNullOrWhiteSpace(_dataStorageService.ExternalLibraryResources?.SelectedStrategyName))
        {
            _notificationManager.Show(new Notification()
            {
                Expiration = TimeSpan.FromSeconds(5),
                Type = NotificationType.Error,
                Message = "Стратегия не выбрана"
            });
            return;
        }

        var strategy = _dataStorageService.ExternalLibraryResources.Strategies
            .FirstOrDefault(s => s.Name.Equals(_dataStorageService.ExternalLibraryResources.SelectedStrategyName));

        if (strategy == null)
        {
            _notificationManager.Show(new Notification()
            {
                Expiration = TimeSpan.FromSeconds(5),
                Type = NotificationType.Error,
                Message = "Некоректное имя выбранной стратегии"
            });
            return;
        }

        var basePath = App.ZapretFolderPath;
        if (string.IsNullOrEmpty(basePath))
        {
            _notificationManager.Show(new Notification()
            {
                Expiration = TimeSpan.FromSeconds(5),
                Type = NotificationType.Error,
                Message = "Ошибка: Путь к корневой папке не указан"
            });
            return;
        }

        var launchProcessResult = await ExternalApplicationService.Launch(basePath, strategy);

        _notificationManager.Show(new Notification()
        {
            Expiration = TimeSpan.FromSeconds(5),
            Type = NotificationType.Success,
            Message = launchProcessResult.Message
        });
    }

    public async Task OnViewLoaded()
    {
        if (_loadIndex != 0)
        {
            if (_dataStorageService.ExternalLibraryResources == null || string.IsNullOrWhiteSpace(_dataStorageService.ExternalLibraryResources.SelectedStrategyName))
            {
                SelectedStrategyName = "Стратегия не выбрана";
                return;
            }

            SelectedStrategyName = _dataStorageService.ExternalLibraryResources.SelectedStrategyName.Split('.').FirstOrDefault();
            _loadIndex++;
            return;
        }

        SelectedStrategyName = "Загружаю";
        await Task.Delay(1000);
        

        if (_dataStorageService.ExternalLibraryResources == null || string.IsNullOrWhiteSpace(_dataStorageService.ExternalLibraryResources.SelectedStrategyName))
        {
            SelectedStrategyName = "Стратегия не выбрана";
            _loadIndex++;
            return;
        }

        SelectedStrategyName = _dataStorageService.ExternalLibraryResources.SelectedStrategyName.Split('.').FirstOrDefault();
        _loadIndex++;
    }
}