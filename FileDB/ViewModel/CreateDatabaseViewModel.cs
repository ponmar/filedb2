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

        private readonly Model.Model model = Model.Model.Instance;

        public CreateDatabaseViewModel()
        {
        }

        [RelayCommand]
        private void Select()
        {
            var result = Dialogs.Default.SelectNewFileDialog("Select new database filename", "db", "db files (*.db)|*.db");
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
                Dialogs.Default.ShowErrorDialog("No database filename specified");
                return;
            }

            if (File.Exists(DatabasePath))
            {
                Dialogs.Default.ShowErrorDialog($"Database {DatabasePath} already exists");
                return;
            }

            if (Dialogs.Default.ShowConfirmDialog($"Create database {DatabasePath}?"))
            {
                try
                {
                    DatabaseUtils.CreateDatabase(DatabasePath);
                    CreatedDatabasePath = DatabasePath;
                    WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
                }
                catch (DatabaseWrapperException e)
                {
                    Dialogs.Default.ShowErrorDialog(e.Message);
                }
            }
        }
    }
}
