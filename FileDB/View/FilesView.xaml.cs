using System.Windows.Controls;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FilesView.xaml
/// </summary>
public partial class FilesView : UserControl
{
    public FilesView()
    {
        InitializeComponent();
        DataContext = new FilesViewModel();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var addedItem in e.AddedItems)
        {
            (addedItem as NewFile)!.IsSelected = true;
        }

        foreach (var addedItem in e.RemovedItems)
        {
            (addedItem as NewFile)!.IsSelected = false;
        }

        (DataContext as FilesViewModel)!.SelectionChanged();
    }

    private void SelectAll_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        NewFilesListView.SelectAll();
    }

    private void DeselectAllButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        NewFilesListView.SelectedItems.Clear();
    }
}
