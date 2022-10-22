using FileDB.ViewModel;
using System;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for BrowseDirectoriesWindow.xaml
    /// </summary>
    public partial class BrowseDirectoriesWindow : Window
    {
        private readonly Model.Model model = Model.Model.Instance;

        public BrowseDirectoriesWindow()
        {
            InitializeComponent();
            DataContext = new BrowseDirectoriesViewModel();

            model.CloseModalDialogRequested += Model_CloseModalDialogRequested;
        }

        private void Model_CloseModalDialogRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            model.CloseModalDialogRequested -= Model_CloseModalDialogRequested;
        }
    }
}
