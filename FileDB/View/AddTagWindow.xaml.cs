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

        model.TagsUpdated += Model_TagsUpdated;
    }

    private void Model_TagsUpdated(object? sender, System.EventArgs e)
    {
        Close();
    }
}
