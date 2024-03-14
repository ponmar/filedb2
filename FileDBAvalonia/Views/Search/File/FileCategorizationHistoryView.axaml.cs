using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search.File;

namespace FileDBAvalonia.Views.Search.File
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
