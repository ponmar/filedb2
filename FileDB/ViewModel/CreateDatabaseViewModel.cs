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
            var result = Dialogs.Instance.SelectNewFileDialog("Select new database filename", "db", "db files (*.db)|*.db");
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
                Dialogs.Instance.ShowErrorDialog("No database filename specified");
                return;
            }

            if (File.Exists(DatabasePath))
            {
                Dialogs.Instance.ShowErrorDialog($"Database {DatabasePath} already exists");
                return;
            }

            if (Dialogs.Instance.ShowConfirmDialog($"Create database {DatabasePath}?"))
            {
                try
                {
                    DatabaseUtils.CreateDatabase(DatabasePath);
                    CreatedDatabasePath = DatabasePath;
                    WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
                }
                catch (DatabaseWrapperException e)
                {
                    Dialogs.Instance.ShowErrorDialog(e.Message);
                }
            }
        }
    }
}
