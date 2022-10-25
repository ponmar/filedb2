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
    private readonly Model.Model model = Model.Model.Instance;

    public AddLocationWindow(int? locationId = null)
    {
        InitializeComponent();
        DataContext = new AddLocationViewModel(locationId);

        WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
        {
            Close();
        });
    }
}
