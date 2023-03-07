using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for AddLocationWindow.xaml
/// </summary>
public partial class AddLocationWindow : Window
{
    public AddLocationWindow(int? locationId = null)
    {
        InitializeComponent();
        DataContext = new AddLocationViewModel(Model.Model.Instance.DbAccess, locationId);

        WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
        {
            Close();
        });
    }
}
