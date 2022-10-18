using FileDB.ViewModel;
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

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for BrowseDirectoriesWindow.xaml
    /// </summary>
    public partial class BrowseDirectoriesWindow : Window
    {
        private readonly Model.Model model = Model.Model.Instance;

        public BrowseDirectoriesWindow()
        {
            InitializeComponent();
            DataContext = new BrowseDirectoriesViewModel();

            model.CloseModalDialogRequested += Model_CloseModalDialogRequested;
        }

        private void Model_CloseModalDialogRequested(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
