using System.Windows;
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
        DataContext = new AddPersonViewModel(
            ServiceLocator.Resolve<IDbAccessRepository>(),
            ServiceLocator.Resolve<IDialogs>(),
            personId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }
}
