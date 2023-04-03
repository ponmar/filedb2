﻿using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for BrowseDirectoriesWindow.xaml
    /// </summary>
    public partial class BrowseDirectoriesWindow : Window
    {
        public BrowseDirectoriesWindow()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<BrowseDirectoriesViewModel>();

            this.RegisterForEvent<CloseModalDialogRequest>((x) => Close());
        }
    }
}
