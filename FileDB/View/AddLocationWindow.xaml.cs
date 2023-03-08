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
        var model = Model.Model.Instance;
        DataContext = new AddLocationViewModel(model.DbAccess, model.Dialogs, locationId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }
}
