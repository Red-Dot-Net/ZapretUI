using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using ZapretUI.ViewModels;

namespace ZapretUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var vm = DataContext as MainWindowViewModel;
        vm?.LoadExternalResources();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        foreach (var process in App.ChildProcesses)
        {
            if (process == null)
                continue;
            
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(500);
                }
            }
            catch { }
            finally
            {
                process.Dispose();
            }
        }

        App.ChildProcesses.Clear();
    }
}