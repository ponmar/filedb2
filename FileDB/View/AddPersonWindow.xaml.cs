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

        model.CloseModalDialogRequested += Model_CloseModalDialogRequested;
    }

    private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
    {
        Close();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        model.CloseModalDialogRequested -= Model_CloseModalDialogRequested;
    }
}
