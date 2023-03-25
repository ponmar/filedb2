using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FindView.xaml
/// </summary>
public partial class FindView : UserControl
{
    public FindView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<FileInfoViewModel>();

        this.RegisterForEvent<ShowImage>((x) =>
        {
            var transformBmp = new TransformedBitmap();
            transformBmp.BeginInit();
            transformBmp.Source = x.Image;
            transformBmp.Transform = new RotateTransform(x.RotateDegrees);
            transformBmp.EndInit();

            CurrentFileImage.Source = transformBmp;
        });

        this.RegisterForEvent<CloseImage>((x) =>
        {
            CurrentFileImage.Source = null;
        });
    }
}
