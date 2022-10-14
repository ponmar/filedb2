using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FindView.xaml
/// </summary>
public partial class FindView : UserControl, IImagePresenter
{
    public FindView()
    {
        InitializeComponent();

        Model.Model.Instance.ImagePresenter = this;
        DataContext = FindViewModel.Instance;
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

    public void CloseImage()
    {
        CurrentFileImage.Source = null;
    }
}
