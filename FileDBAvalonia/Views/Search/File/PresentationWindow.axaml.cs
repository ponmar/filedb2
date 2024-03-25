using Avalonia.Controls;
using FileDBAvalonia.ViewModels.Search;

namespace FileDBAvalonia.Views.Search.File
{
    public partial class PresentationWindow : Window
    {
        public PresentationWindow()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<FileViewModel>();
        }
    }
}
