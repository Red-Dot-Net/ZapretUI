using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    readonly DataStorageService _dataStorageService;

    [ObservableProperty]
    public partial string? SelectedStrategyNameDisplay {  get; set; }

    public ObservableCollection<string> StrategyNames { get; set; } = [];

    [ObservableProperty]
    public partial string? SelectedStrategyName { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateChackActive { get; set; } = true;


    public SettingsWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
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

    public void OnViewLoaded()
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
        catch { }
    }
}