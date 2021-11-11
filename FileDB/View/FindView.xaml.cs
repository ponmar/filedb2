using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileDB2Browser.ViewModel;

namespace FileDB2Browser.View
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
