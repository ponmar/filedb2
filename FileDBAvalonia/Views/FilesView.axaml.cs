using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views
{
    public partial class FilesView : UserControl
    {
        public FilesView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<FilesViewModel>();
            }
        }
    }
}
