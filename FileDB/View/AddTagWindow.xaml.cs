using System.Windows;
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
        DataContext = new AddTagViewModel(tagId);
    }
}
