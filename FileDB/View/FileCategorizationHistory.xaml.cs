using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for FileCategorizationHistory.xaml
    /// </summary>
    public partial class FileCategorizationHistory : UserControl
    {
        public FileCategorizationHistory()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
        }
    }
}
