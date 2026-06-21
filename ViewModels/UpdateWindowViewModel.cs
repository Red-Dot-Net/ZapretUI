using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZapretUI.Helpers;
using ZapretUI.Services;

namespace ZapretUI.ViewModels;

public partial class UpdateWindowViewModel : ViewModelBase
{
    readonly DataStorageService _dataStorageService;
    IStorageFile? _dropedFile;

    [ObservableProperty]
    public partial string? CurrentLoadedVersion { get; set; }

    [ObservableProperty]
    public partial string? CurrentServerVersion { get; set; }

    [ObservableProperty]
    public partial string? DownloadUrl { get; set; }

    [ObservableProperty]
    public partial string? DownloadUrlErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsDownloadPanelVisible { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateButtonVisible { get; set; }

    [ObservableProperty]
    public partial bool IsDownloadErrorTextVisible { get; set; }

    [ObservableProperty]
    public partial string? DownloadFolderPath { get; set; }

    [ObservableProperty]
    public partial string? InfoText { get; set; }

    [ObservableProperty]
    public partial bool IsInfoTextVisible { get; set; }

    public UpdateWindowViewModel()
    {
        _dataStorageService = App.DataStorageService;

        if (string.IsNullOrWhiteSpace(App.LoadedGitHubVersion))
            CurrentServerVersion = "Версия на GitHub не загружена.";
        else
            CurrentServerVersion = $"Последняя версия на GitHub: {App.LoadedGitHubVersion}";

        IsDownloadPanelVisible = false;
        IsDownloadErrorTextVisible = false;
        IsUpdateButtonVisible = false;
        IsInfoTextVisible = false;
    }

    [RelayCommand]
    private async Task CheckForNewVersion()
    {
        var res = await SourceDownloadService.GetLatestVersion(App.GitHubSourceDirectory);
        if (res == null)
        {
            CurrentServerVersion = "Ошибка загрузки последней версии";
            return;
        }

        if (res is Error error)
        {
            CurrentServerVersion = $"Ошибка загрузки последней версии: {error.Message}";
            return;
        }

        if (res is Success success && success.Value is string ver)
        {
            CurrentServerVersion = $"Последняя версия на GitHub: {ver}";
            App.LoadedGitHubVersion = ver;
        }

        if (_dataStorageService.ExternalLibraryResources.CurrentSourceVersion != null &&
            _dataStorageService.ExternalLibraryResources.CurrentSourceVersion
            .Equals(App.LoadedGitHubVersion))
            return;

        var downloadUrlResult = await SourceDownloadService.GetLatestVersionReleaseDownloadUrl("Flowseal", "zapret-discord-youtube");
        if (downloadUrlResult is Error err)
        {
            DownloadUrlErrorMessage = err.Message;
            IsDownloadErrorTextVisible = true;
            IsDownloadPanelVisible = false;
            return;
        }

        if (downloadUrlResult is Success s)
        {
            IsDownloadErrorTextVisible = false;
            IsDownloadPanelVisible = true;
            DownloadUrl = s.Value as string;
        }
    }

    [RelayCommand]
    private async Task GetDownloadInfo()
    {
        var res = await SourceDownloadService.GetLatestVersion(App.GitHubSourceDirectory);
        if (res == null)
        {
            CurrentServerVersion = "Ошибка загрузки последней версии";
            return;
        }

        if (res is Error error)
        {
            CurrentServerVersion = $"Ошибка загрузки последней версии: {error.Message}";
            return;
        }

        if (res is Success success && success.Value is string ver)
        {
            CurrentServerVersion = $"Последняя версия на GitHub: {ver}";
            App.LoadedGitHubVersion = ver;
        }

        var downloadUrlResult = await SourceDownloadService.GetLatestVersionReleaseDownloadUrl("Flowseal", "zapret-discord-youtube");
        if (downloadUrlResult is Error err)
        {
            DownloadUrlErrorMessage = err.Message;
            IsDownloadErrorTextVisible = true;
            IsDownloadPanelVisible = false;
            return;
        }

        if (downloadUrlResult is Success s)
        {
            IsDownloadErrorTextVisible = false;
            IsDownloadPanelVisible = true;
            DownloadUrl = s.Value as string;
        }
    }

    [RelayCommand]
    private async Task SetDownloadFolderPath()
    {
        if (App.AppTopLevel == null)
            return;

        var topLevel = App.AppTopLevel;
        if (topLevel == null)
            return;

        IStorageFolder? prevSelection = null;
        string? path = _dataStorageService.ExternalLibraryResources?.DownloadFolderPath;

        if (path != null)
        {
            try
            {
                var uri = new Uri(path);
                prevSelection = await topLevel.StorageProvider.TryGetFolderFromPathAsync(uri);
            }
            catch { }
        }
        else if (DownloadFolderPath is string txt && !string.IsNullOrWhiteSpace(txt))
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

        DownloadFolderPath = folders[0].Path.LocalPath;

        await _dataStorageService.SaveDownloadFolderPath(folders[0].Path.LocalPath);
    }

    [RelayCommand]
    private async Task Update()
    {
        if (_dropedFile == null)
            return;

        IsInfoTextVisible = true;

        var result = await UpdateService.ExtractZipAsync(_dropedFile);
        if (result is Error error)
        {
            InfoText = error.Message;
            return;
        }

        try
        {
            UpdateService.DeleteContents(App.SourceFilesBaseDirectory);
        }
        catch 
        {
            InfoText = "Возникла ошибка при удалении содержимого корневой папки";
            return;
        }

        try
        {
            var folderInArchive = Directory.GetDirectories(Path.Combine(App.SourceFilesBaseDirectory, "temp")).First();
            UpdateService.CopyDirectory(folderInArchive, App.SourceFilesBaseDirectory);
        }
        catch 
        {
            InfoText = "Возникла ошибка при копировании содержимого временной папки";
            return;
        }

        try
        {
            var dir = Path.Combine(App.SourceFilesBaseDirectory, "temp");
            Directory.Delete(dir, true);
        }
        catch 
        {
            InfoText = "Возникла ошибка при удалении временной папки";
            return;
        }

        InfoText = "Успешно обновлено";
    }

    public void ChangeInfoText(string text)
    {
        InfoText = text;
    }

    public async Task OnViewLoaded()
    {
        var loadedVersion = _dataStorageService.ExternalLibraryResources.CurrentSourceVersion;

        if (string.IsNullOrWhiteSpace(loadedVersion))
        {
            var version = FileParserService.GetSourceVersion(App.SourceFilesBaseDirectory);
            if (version != null)
                _dataStorageService?.SaveCurrentSourceVersion(version);

            CurrentLoadedVersion = $"Текущая версия: {version}";
            return;
        }

        CurrentLoadedVersion = $"Текущая версия: {loadedVersion}";
    }

    public async Task OnFileDrop(IStorageItem? file)
    {
        if (file == null || !file.Name.EndsWith(".zip"))
            return;

        _dropedFile = file as IStorageFile;
        IsUpdateButtonVisible = true;
    }
}
