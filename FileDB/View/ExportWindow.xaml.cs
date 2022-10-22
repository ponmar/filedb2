using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly Model.Model model = Model.Model.Instance;

        public ExportWindow()
        {
            InitializeComponent();
            DataContext = new ExportViewModel();

            model.CloseModalDialogRequested += Model_CloseModalDialogRequested; ;
        }

        private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            model.CloseModalDialogRequested -= Model_CloseModalDialogRequested;
        }
    }
}
