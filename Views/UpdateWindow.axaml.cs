using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;
using ZapretUI.ViewModels;
namespace ZapretUI.Views;

public partial class UpdateWindow : UserControl
{
    public UpdateWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not UpdateWindowViewModel vm)
            return;

        await vm.OnViewLoaded();
    }

    private async void OnDragOver(object? sender, DragEventArgs e)
    {
        DragAndDropInfoMessageBlock.IsVisible = true;
        DragAndDropInfoMessageBlock.Text = "Перетащите архив в это окно";
        DragAndDropFileNameBlock.IsVisible = false;

        if (DataContext is not UpdateWindowViewModel)
            return;

        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private async void OnLeave(object? sender, DragEventArgs e)
    {
        DragAndDropInfoMessageBlock.IsVisible = true;
        DragAndDropInfoMessageBlock.Text = "Перетащите архив в это окно";
        DragAndDropFileNameBlock.IsVisible = false;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Formats.Contains(DataFormat.File))
            return;

        var files = e.DataTransfer.TryGetFiles();
        if (files != null && files.Length != 0)
        {
            DragAndDropInfoMessageBlock.IsVisible = false;
            DragAndDropFileNameBlock.IsVisible = true;
            DragAndDropFileNameBlock.Text = files[0].Path.ToString().Split('/').Last();

            if (DataContext is not UpdateWindowViewModel vm)
                return;

            vm.ChangeInfoText(files[0].Path.ToString());

            await vm.OnFileDrop(files[0]);
        }
    }
}
