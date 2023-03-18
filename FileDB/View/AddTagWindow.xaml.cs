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
        DataContext = new AddTagViewModel(
            ServiceLocator.Resolve<IDbAccessRepository>(),
            ServiceLocator.Resolve<IDialogs>(),
            tagId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }

    private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
    {
        Close();
    }
}
