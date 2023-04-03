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
            DataContext = ServiceLocator.Resolve<ExportViewModel>();

            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
