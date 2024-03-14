using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search.File;

namespace FileDBAvalonia.Views.Search.File
{
    public partial class FileCategorizationHistory : UserControl
    {
        public FileCategorizationHistory()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
            }
        }
    }
}
