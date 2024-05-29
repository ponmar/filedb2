using Avalonia.Controls;
using FileDB.ViewModels.Search.File;

namespace FileDB.Views.Search.File
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
