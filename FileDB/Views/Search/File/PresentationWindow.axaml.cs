using Avalonia.Controls;
using FileDB.ViewModels.Search;

namespace FileDB.Views.Search.File
{
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
                SystemDecorations = SystemDecorations.Full;
            }
            else
            {
                WindowState = WindowState.FullScreen;
                SystemDecorations = SystemDecorations.None;
            }
        }
    }
}
