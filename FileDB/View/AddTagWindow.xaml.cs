using System.Windows;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for AddTagWindow.xaml
/// </summary>
public partial class AddTagWindow : Window
{
    public AddTagWindow(int? tagId = null)
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<AddTagViewModel>("tagId", tagId);

        this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
    }

    private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
    {
        Close();
    }
}
