using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views
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
