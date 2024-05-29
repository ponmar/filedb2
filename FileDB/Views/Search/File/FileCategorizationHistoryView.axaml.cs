using Avalonia.Controls;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File
{
    public partial class FileCategorizationHistoryView : UserControl
    {
        public FileCategorizationHistoryView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
            }
        }
    }
}
