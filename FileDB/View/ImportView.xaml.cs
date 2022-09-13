using System.Collections.Generic;
using System.Windows.Controls;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportView : UserControl
    {
        public ImportView()
        {
            InitializeComponent();
            DataContext = new ImportViewModel();
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

            (DataContext as ImportViewModel)!.SelectionChanged();
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
}
