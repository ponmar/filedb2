using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class ExportSearchResultWindow : Window
    {
        public ExportSearchResultWindow()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<ExportSearchResultViewModel>();
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
