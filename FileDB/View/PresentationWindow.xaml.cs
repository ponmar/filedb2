using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileDB.View;

/// <summary>
/// Interaction logic for PresentationWindow.xaml
/// </summary>
public partial class PresentationWindow : Window
{
    public PresentationWindow()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;

        this.RegisterForEvent<ShowImage>((x) => ShowImage(x.Image, x.RotateDegrees));

        this.RegisterForEvent<CloseImage>((x) =>
        {
            CurrentFileImage.Source = null;
        });
    }

    public void ShowImage(BitmapImage image, double rotateDegrees)
    {
        var transformBmp = new TransformedBitmap();
        transformBmp.BeginInit();
        transformBmp.Source = image;
        transformBmp.Transform = new RotateTransform(rotateDegrees);
        transformBmp.EndInit();

        CurrentFileImage.Source = transformBmp;
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
