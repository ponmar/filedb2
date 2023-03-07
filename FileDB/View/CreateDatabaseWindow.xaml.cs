﻿using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.ViewModel;
using System.Windows;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for CreateDatabaseWindow.xaml
    /// </summary>
    public partial class CreateDatabaseWindow : Window
    {
        public CreateDatabaseWindow()
        {
            InitializeComponent();
            var model = Model.Model.Instance;
            DataContext = new CreateDatabaseViewModel(model.Dialogs);

            WeakReferenceMessenger.Default.Register<CloseModalDialogRequested>(this, (r, m) =>
            {
                Close();
            });
        }
    }
}
