using System.Windows;
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
        DataContext = ServiceLocator.Resolve<AddLocationViewModel>("locationId", locationId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }
}
