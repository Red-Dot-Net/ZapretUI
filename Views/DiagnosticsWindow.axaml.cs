using Avalonia.Controls;
using Avalonia.Interactivity;
using ZapretUI.ViewModels;
namespace ZapretUI.Views;

public partial class DiagnosticsWindow : UserControl
{
    public DiagnosticsWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not DiagnosticsWindowViewModel vm)
            return;

        await vm.OnLoadWindow();
    }
}
