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
    private readonly Model.Model model = Model.Model.Instance;

    public AddPersonWindow(int? personId = null)
    {
        InitializeComponent();
        DataContext = new AddPersonViewModel(personId);

        WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
        {
            Close();
        });
    }
}
