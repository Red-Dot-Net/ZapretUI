using Avalonia.Platform.Storage;
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

    [ObservableProperty]
    public partial string? FolderText { get; set; }

    [ObservableProperty]
    public partial string? InfoText {  get; set; }

    [ObservableProperty]
    public partial bool IsCheckboxVisible { get; set; }

    public ObservableCollection<string> StrategyNames { get; set; } = [];

    [ObservableProperty]
    public partial string? SelectedStrategyName {  get; set; }


    public SettingsWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;
        FolderText = "";
        InfoText = "";
    }

    [RelayCommand]
    private async Task OpenFileExplorer()
    {
        if (App.AppTopLevel == null)
            return;
        
        var topLevel = App.AppTopLevel;
        if (topLevel == null)
            return;

        IStorageFolder? prevSelection = null;
        string? path = _dataStorageService.ExternalLibraryResources?.FolderPath;
        InfoText = path;

        if (path != null)
        {
            try
            {
                var uri = new Uri(path);
                prevSelection = await topLevel.StorageProvider.TryGetFolderFromPathAsync(uri);
            }
            catch { }
        }
        else if (FolderText is string txt && !string.IsNullOrWhiteSpace(txt))
        {
            try
            {
                var uri = new Uri(txt);
                prevSelection = await topLevel.StorageProvider.TryGetFolderFromPathAsync(uri);
            }
            catch { }
        }

        var folderPickerOptions = new FolderPickerOpenOptions
        {
            Title = "Select your file",
            AllowMultiple = false
        };

        if (prevSelection != null)
        {
            folderPickerOptions.SuggestedStartLocation = prevSelection;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);

        if (folders == null || folders.Count == 0)
            return;

        FolderText = folders[0].Path.LocalPath;

        await _dataStorageService.SaveBasePath(folders[0].Path.LocalPath);
    }

    [RelayCommand]
    private async Task ScanAndSaveStrategies()
    {
        if (string.IsNullOrWhiteSpace(_dataStorageService.ExternalLibraryResources.FolderPath))
            return;

        var strategies = FileParserService.LoadAllStrategies(_dataStorageService.ExternalLibraryResources.FolderPath);
        await _dataStorageService.SaveStrategies(strategies);
        StrategyNames.Clear();
        foreach (var s in strategies)
        {
            StrategyNames.Add(s.Name);
            App.LoadedStrategies.Add(s);
        }
    }

    partial void OnFolderTextChanged(string? value)
    {
        if (value is string text &&
            !string.IsNullOrEmpty(text) &&
            File.Exists(text + "\\bin\\winws.exe"))
        {
            IsCheckboxVisible = true;
            return;
        }

        IsCheckboxVisible = false;
    }

    async partial void OnSelectedStrategyNameChanged(string? value)
    {
        if (value == null)
            return;

        _dataStorageService.ExternalLibraryResources.SelectedStrategyName = value;
        await _dataStorageService.SaveSelectedStrategyName(value);

        InfoText = value;
    }

    public void OnViewLoaded()
    {
        if (_dataStorageService.ExternalLibraryResources == null)
        {
            IsCheckboxVisible = false;
            return;
        }

        FolderText = _dataStorageService.ExternalLibraryResources.FolderPath;

        if (!IsInCorrectFolder())
        {
            IsCheckboxVisible = false;
            return;
        }

        InfoText = _dataStorageService.ExternalLibraryResources.FolderPath;
        IsCheckboxVisible = true;

        StrategyNames.Clear();
        foreach (var strategy in _dataStorageService.ExternalLibraryResources.Strategies)
        {
            StrategyNames.Add(strategy.Name);
        }

        SelectedStrategyName = _dataStorageService.ExternalLibraryResources.SelectedStrategyName;
    }

    private bool IsInCorrectFolder()
    {
        return File.Exists(_dataStorageService.ExternalLibraryResources.FolderPath + "\\bin\\winws.exe");
    }
}