using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels.Search;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views.Search.File;

[ExcludeFromCodeCoverage]
public partial class PresentationWindow : Window
{
    public PresentationWindow()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<FileViewModel>();
    }

    private void Window_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (WindowState == WindowState.FullScreen)
        {
            WindowState = WindowState.Normal;
            WindowDecorations = WindowDecorations.Full;
        }
        else
        {
            WindowState = WindowState.FullScreen;
            WindowDecorations = WindowDecorations.None;
        }
    }
}
