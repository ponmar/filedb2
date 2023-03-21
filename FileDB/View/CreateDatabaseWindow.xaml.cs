using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for CreateDatabaseWindow.xaml
    /// </summary>
    public partial class CreateDatabaseWindow : Window
    {
        public CreateDatabaseWindow()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<CreateDatabaseViewModel>();

            this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
        }
    }
}
