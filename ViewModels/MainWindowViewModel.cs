using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using ZapretUI.Services;

namespace ZapretUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DataStorageService _dataStorageService;

        [ObservableProperty]
        public partial object CurrentView { get; set; }

        [ObservableProperty]
        public partial string? CurrentLoadedVersion { get; set; }

        public MainWindowViewModel()
        {
            _dataStorageService = App.DataStorageService;

            CurrentView = new LaunchWindowViewModel();
        }

        [RelayCommand]
        private void SwitchToLaunchWindow()
        {
            CurrentView = new LaunchWindowViewModel();
        }

        [RelayCommand]
        private void SwitchToSettingWindow()
        {
            CurrentView = new SettingsWindowViewModel();
        }

        [RelayCommand]
        private void SwitchToUpdateWindow()
        {
            CurrentView = new UpdateWindowViewModel();
        }

        [RelayCommand]
        private void SwitchToListsWindow()
        {
            CurrentView = new ListsWindowViewModel();
        }

        public async Task LoadExternalResources()
        {
            await _dataStorageService.LoadResources(Path.Combine(AppContext.BaseDirectory, App.SaveFileName));
            await Task.Delay(1000);

            var basePath = _dataStorageService.ExternalLibraryResources.FolderPath;
            if (string.IsNullOrEmpty(basePath))
                return;

            var version = FileParserService.GetSourceVersion(basePath);
            if (version != null)
                _dataStorageService?.SaveCurrentSourceVersion(version);

            CurrentLoadedVersion = version;
        }
    }
}
