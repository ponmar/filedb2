using System.Diagnostics;
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
        }

        public void ShowImage(BitmapImage image)
        {
            CurrentFileImage.Source = image;
        }

        private void OpenLocationUri(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Utils.OpenUriInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
