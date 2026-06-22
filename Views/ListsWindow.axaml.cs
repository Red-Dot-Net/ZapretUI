using Avalonia.Controls;
using Avalonia.Interactivity;
using ZapretUI.ViewModels;
namespace ZapretUI.Views;

public partial class ListsWindow : UserControl
{
    public ListsWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not ListsWindowViewModel vm)
            return;
    }
}
