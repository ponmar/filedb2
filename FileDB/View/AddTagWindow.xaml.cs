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
    private readonly Model.Model model = Model.Model.Instance;

    public AddTagWindow(int? tagId = null)
    {
        InitializeComponent();
        DataContext = new AddTagViewModel(tagId);

        WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
        {
            Close();
        });
    }

    private void Model_CloseModalDialogRequested(object? sender, System.EventArgs e)
    {
        Close();
    }
}
