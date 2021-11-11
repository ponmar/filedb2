using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for FindView.xaml
    /// </summary>
    public partial class FindView : UserControl, IImagePresenter
    {
        public FindView()
        {
            InitializeComponent();
            DataContext = new FindViewModel(this);
            // TODO: this must also be done when page becomes active
            Loaded += (s, e) => Keyboard.Focus(this);
        }

        public void ShowImage(BitmapImage image)
        {
            CurrentFileImage.Source = image;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
