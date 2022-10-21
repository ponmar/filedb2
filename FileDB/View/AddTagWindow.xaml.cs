using System.Windows;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for AddTagWindow.xaml
/// </summary>
public partial class AddTagWindow : Window
{
    private readonly Model.Model model = Model.Model.Instance;

    public AddTagWindow(int? tagId = null)
    {
        InitializeComponent();
        DataContext = new AddTagViewModel(tagId);

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
