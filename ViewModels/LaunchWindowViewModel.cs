using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ZapretUI.Helpers;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class LaunchWindowViewModel : ViewModelBase
{
    readonly DataStorageService _dataStorageService;
    static int _loadIndex = 0;

    [ObservableProperty]
    public partial object OperationResultMessage { get; set; }

    [ObservableProperty]
    public partial bool IsOperationResultBorderVisible { get; set; }

    [ObservableProperty]
    public partial string? SelectedStrategyName {  get; set; }

    public LaunchWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
        OperationResultMessage = "";
        SelectedStrategyName = "";
        IsOperationResultBorderVisible = false;
    }

    [RelayCommand]
    private async Task Launch()
    {
        IsOperationResultBorderVisible = true;

        if (string.IsNullOrWhiteSpace(_dataStorageService.ExternalLibraryResources?.SelectedStrategyName))
        {
            OperationResultMessage = "Стратегия не выбрана";
            return;
        }

        var strategy = _dataStorageService.ExternalLibraryResources.Strategies
            .FirstOrDefault(s => s.Name.Equals(_dataStorageService.ExternalLibraryResources.SelectedStrategyName));

        if (strategy == null)
        {
            OperationResultMessage = "Некоректное имя выбранной стратегии";
            return;
        }

        if (!ExternalApplicationService.AreTcpTimestampsEnabled())
        {
            ExternalApplicationService.EnableTcpTimestamps();
        }

        ExternalApplicationService.KillExistingProcesses("winws");
        var launchProcessResult = await ExternalApplicationService.Launch(App.SourceFilesBaseDirectory, strategy);

        if (launchProcessResult is Success success && success.Value is Process process)
        {
            App.ChildProcesses.Add(process);
        }
        
        OperationResultMessage = launchProcessResult.Message;
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