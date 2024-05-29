using Avalonia.Controls;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;

namespace FileDB.Views.Dialogs
{
    public partial class ExportSearchResultWindow : Window
    {
        public ExportSearchResultWindow()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<ExportSearchResultViewModel>();
            }
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
