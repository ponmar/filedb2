using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for AddPersonWindow.xaml
/// </summary>
public partial class AddPersonWindow : Window
{
    public AddPersonWindow(int? personId = null)
    {
        InitializeComponent();
        var model = Model.Model.Instance;
        DataContext = new AddPersonViewModel(model.DbAccess, model.Dialogs, personId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }
}
