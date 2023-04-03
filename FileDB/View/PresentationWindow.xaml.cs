using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View;

/// <summary>
/// Interaction logic for PresentationWindow.xaml
/// </summary>
public partial class PresentationWindow : Window
{
    public PresentationWindow()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<FileInfoViewModel>();
    }

    private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (WindowStyle == WindowStyle.None)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }
    }
}
