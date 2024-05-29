using Avalonia.Controls;
using FileDB.Model;
using FileDB.ViewModels.Dialogs;

namespace FileDB.Views.Dialogs
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
