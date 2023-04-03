using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace FileDB.Model;

public class ImageUtils
{
    public static ImageSource Rotate(BitmapImage image, int rotateDegrees)
    {
        if (rotateDegrees == 0)
        {
            return image;
        }

        var transformBmp = new TransformedBitmap();
        transformBmp.BeginInit();
        transformBmp.Source = image;
        transformBmp.Transform = new RotateTransform(rotateDegrees);
        transformBmp.EndInit();

        return transformBmp;
    }
}
