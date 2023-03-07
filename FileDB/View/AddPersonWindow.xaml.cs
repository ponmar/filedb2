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
        DataContext = new AddPersonViewModel(Model.Model.Instance.DbAccess, personId);

        WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
        {
            Close();
        });
    }
}
