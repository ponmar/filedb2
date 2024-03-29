using Avalonia.Controls;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Views.Dialogs
{
    public partial class BrowseSubDirectoriesWindow : Window
    {
        public BrowseSubDirectoriesWindow()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = ServiceLocator.Resolve<BrowseSubDirectoriesViewModel>();
            }
            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
