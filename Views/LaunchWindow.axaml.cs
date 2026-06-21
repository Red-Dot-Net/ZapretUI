using Avalonia.Controls;
using Avalonia.Interactivity;
using ZapretUI.ViewModels;
namespace ZapretUI.Views;

public partial class LaunchWindow : UserControl
{
    public LaunchWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not LaunchWindowViewModel vm)
            return;

        await vm.OnViewLoaded();
    }
}
