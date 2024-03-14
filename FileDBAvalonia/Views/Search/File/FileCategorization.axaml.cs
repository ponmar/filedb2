using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search.File;

namespace FileDBAvalonia.Views.Search.File
{
    public partial class FileCategorization : UserControl
    {
        public FileCategorization()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<FileCategorizationViewModel>();
            }
        }
    }
}
