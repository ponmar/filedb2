using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search.File;

namespace FileDBAvalonia.Views.Search.File
{
    public partial class FileCategorizationView : UserControl
    {
        public FileCategorizationView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
            }
        }
    }
}
