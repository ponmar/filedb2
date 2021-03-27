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
    /// Interaction logic for LocationsView.xaml
    /// </summary>
    public partial class LocationsView : UserControl
    {
        public LocationsView()
        {
            InitializeComponent();
            DataContext = new LocationsViewModel(Utils.FileDB2Handle);
        }
    }
}
