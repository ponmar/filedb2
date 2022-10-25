using CommunityToolkit.Mvvm.Messaging;
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
        private readonly Model.Model model = Model.Model.Instance;

        public CreateDatabaseWindow()
        {
            InitializeComponent();
            DataContext = new CreateDatabaseViewModel();

            WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
            {
                Close();
            });
        }
    }
}
