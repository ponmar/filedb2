using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow()
        {
            InitializeComponent();
            DataContext = new ExportViewModel(
                ServiceLocator.Resolve<IDialogs>(),
                ServiceLocator.Resolve<IDbAccessRepository>(),
                ServiceLocator.Resolve<IFilesystemAccessRepository>());

            this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
        }
    }
}
