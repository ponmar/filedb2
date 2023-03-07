using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using System.IO;

namespace FileDB.ViewModel
{
    public partial class CreateDatabaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string databasePath = string.Empty;

        public string CreatedDatabasePath = string.Empty;

        private readonly IDialogs dialogs;

        public CreateDatabaseViewModel(IDialogs dialogs)
        {
            this.dialogs = dialogs;
        }

        [RelayCommand]
        private void Select()
        {
            var result = dialogs.SelectNewFileDialog("Select new database filename", "db", "db files (*.db)|*.db");
            if (result != null)
            {
                DatabasePath = result;
            }
        }

        [RelayCommand]
        private void Create()
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                dialogs.ShowErrorDialog("No database filename specified");
                return;
            }

            if (File.Exists(DatabasePath))
            {
                dialogs.ShowErrorDialog($"Database {DatabasePath} already exists");
                return;
            }

            if (dialogs.ShowConfirmDialog($"Create database {DatabasePath}?"))
            {
                try
                {
                    DatabaseSetup.CreateDatabase(DatabasePath);
                    CreatedDatabasePath = DatabasePath;
                    WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
                }
                catch (DatabaseWrapperException e)
                {
                    dialogs.ShowErrorDialog(e.Message);
                }
            }
        }
    }
}
