using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for BrowseDirectoriesWindow.xaml
    /// </summary>
    public partial class BrowseDirectoriesWindow : Window
    {
        public BrowseDirectoriesWindow()
        {
            InitializeComponent();
            DataContext = new BrowseDirectoriesViewModel(Model.Model.Instance.DbAccess);

            WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
            {
                Close();
            });
        }
    }
}
