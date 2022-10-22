using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            var result = Dialogs.SelectNewFileDialog("Select new database filename", "db", "db files (*.db)|*.db");
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
                Dialogs.ShowErrorDialog("No database filename specified");
                return;
            }

            if (File.Exists(DatabasePath))
            {
                Dialogs.ShowErrorDialog($"Database {DatabasePath} already exists");
                return;
            }

            if (Dialogs.ShowConfirmDialog($"Create database {DatabasePath}?"))
            {
                try
                {
                    DatabaseUtils.CreateDatabase(DatabasePath);
                    CreatedDatabasePath = DatabasePath;
                    model.RequestCloseModalDialog();
                }
                catch (DatabaseWrapperException e)
                {
                    Dialogs.ShowErrorDialog(e.Message);
                }
            }
        }
    }
}
