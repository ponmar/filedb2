using System.Windows;
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

        model.PersonsUpdated += Model_PersonsUpdated;
    }

    private void Model_PersonsUpdated(object? sender, System.EventArgs e)
    {
        Close();
    }
}
