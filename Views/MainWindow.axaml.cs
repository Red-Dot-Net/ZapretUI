using Avalonia.Controls;
using Avalonia.Interactivity;
using ZapretUI.Services;
using ZapretUI.ViewModels;

namespace ZapretUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //Closing += OnClosing;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var vm = DataContext as MainWindowViewModel;
        vm?.LoadExternalResources();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (e.CloseReason != WindowCloseReason.ApplicationShutdown &&
            e.CloseReason != WindowCloseReason.OSShutdown)
        {
            e.Cancel = true;
            Hide();
        }
        else
            base.OnClosing(e);
    }

    //private void OnClosing(object? sender, WindowClosingEventArgs e)
    //{
    //    ExternalApplicationService.KillExistingProcesses("winws");
    //}
}