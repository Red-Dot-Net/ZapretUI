using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ZapretUI.ViewModels;

public partial class ListsWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? HostsAdreses { get; set; } = "";

    [ObservableProperty]
    public partial bool IsHostsLoaded { get; set; } = false;
    
    [RelayCommand]
    private async Task LoadHostsFromGitHub()
    {
        var httpClient = App.HttpClient;

        var response = await httpClient.GetStringAsync("https://raw.githubusercontent.com/Flowseal/zapret-discord-youtube/refs/heads/main/.service/hosts");
        if (string.IsNullOrEmpty(response))
        {
            IsHostsLoaded = false;
            return;
        }

        IsHostsLoaded = true;
        HostsAdreses = response;
    }

    [RelayCommand]
    private async Task SaveToBuffer()
    {
        if (App.AppTopLevel?.Clipboard == null)
            return;

        await App.AppTopLevel.Clipboard.SetTextAsync(HostsAdreses);
    }

    [RelayCommand]
    private void OpenHostsDirectory()
    {
        var path = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        Process.Start("explorer.exe", $"/select,\"{path}\"");
    }
}
