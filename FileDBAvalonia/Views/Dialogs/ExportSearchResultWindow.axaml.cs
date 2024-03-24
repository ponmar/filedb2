using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class ExportSearchResultWindow : Window
    {
        public ExportSearchResultWindow()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<ExportSearchResultViewModel>();
        }
    }
}
