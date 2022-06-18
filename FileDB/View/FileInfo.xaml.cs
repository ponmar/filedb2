using FileDB.ViewModel;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for FileInfo.xaml
    /// </summary>
    public partial class FileInfo : UserControl
    {
        public FileInfo()
        {
            InitializeComponent();
            DataContext = FindViewModel.Instance;
        }

        private void OpenLocationUri(object sender, RequestNavigateEventArgs e)
        {
            Utils.OpenUriInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
