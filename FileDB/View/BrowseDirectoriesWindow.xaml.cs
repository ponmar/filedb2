using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
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

            WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
            {
                Close();
            });
        }
    }
}
