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
        DataContext = new AddLocationViewModel(ServiceLocator.Resolve<IDbAccessRepository>(), ServiceLocator.Resolve<IDialogs>(), locationId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }
}
