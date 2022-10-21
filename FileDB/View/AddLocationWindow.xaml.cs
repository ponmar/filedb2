using System.Windows;
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
