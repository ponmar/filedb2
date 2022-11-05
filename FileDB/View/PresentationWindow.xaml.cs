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

        WeakReferenceMessenger.Default.Register<ShowImage>(this, (r, m) =>
        {
            var transformBmp = new TransformedBitmap();
            transformBmp.BeginInit();
            transformBmp.Source = m.Image;
            transformBmp.Transform = new RotateTransform(m.RotateDegrees);
            transformBmp.EndInit();

            CurrentFileImage.Source = transformBmp;
        });

        WeakReferenceMessenger.Default.Register<CloseImage>(this, (r, m) =>
        {
            CurrentFileImage.Source = null;
        });
    }
}
