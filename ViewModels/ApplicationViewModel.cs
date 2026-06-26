using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using ZapretUI.Views;

namespace ZapretUI.ViewModels;

public partial class ApplicationViewModel : ViewModelBase
{
    [RelayCommand]
    private static void ShowWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
           var mainWindow = desktop.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Show();
                mainWindow.Activate();
            }
        }
    }

    [RelayCommand]
    private static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
