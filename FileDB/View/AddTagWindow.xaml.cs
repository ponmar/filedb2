using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
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
        var model = Model.Model.Instance;
        DataContext = new AddTagViewModel(model.DbAccess, model.Dialogs, tagId);

        this.RegisterForEvent<CloseModalDialogRequested>((x) => Close());
    }

    private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
    {
        Close();
    }
}
