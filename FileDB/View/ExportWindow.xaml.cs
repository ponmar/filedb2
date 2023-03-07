using CommunityToolkit.Mvvm.Messaging;
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
        private readonly Model.Model model = Model.Model.Instance;

        public ExportWindow()
        {
            InitializeComponent();
            var model = Model.Model.Instance;
            DataContext = new ExportViewModel(model.Dialogs);

            WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
            {
                Close();
            });
        }
    }
}
