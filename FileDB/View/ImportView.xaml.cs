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

            (DataContext as ImportViewModel).SelectionChanged();
        }
    }
}
