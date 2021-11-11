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
using System.Windows.Shapes;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for AddTagWindow.xaml
    /// </summary>
    public partial class AddTagWindow : Window
    {
        public AddTagWindow(int? tagId = null)
        {
            InitializeComponent();
            DataContext = new AddTagViewModel(tagId);
        }
    }
}
