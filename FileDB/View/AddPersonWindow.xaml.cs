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
        DataContext = ServiceLocator.Resolve<AddPersonViewModel>("personId", personId);

        this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
    }
}
