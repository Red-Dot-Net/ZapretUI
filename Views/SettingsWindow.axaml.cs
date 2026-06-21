using Avalonia.Controls;
using Avalonia.Interactivity;
using ZapretUI.ViewModels;

namespace ZapretUI.Views;

public partial class SettingsWindow : UserControl
{
    public SettingsWindow()
    {
        InitializeComponent();

        DataContext = new SettingsWindowViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not SettingsWindowViewModel vm)
            return;

        vm.OnViewLoaded();
    }
}
