using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for FileBrowseControls.xaml
    /// </summary>
    public partial class FileBrowseControls : UserControl
    {
        public FileBrowseControls()
        {
            InitializeComponent();
            DataContext = FindViewModel.Instance;
        }
    }
}
