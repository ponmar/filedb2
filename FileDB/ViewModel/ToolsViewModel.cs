using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using TextCopy;

namespace FileDB.ViewModel
{
    public class ToolsViewModel : ViewModelBase
    {
        public ICommand CreateBackupCommand => createBackupCommand ??= new CommandHandler(CreateBackup);
        private ICommand createBackupCommand;

        public ICommand DatabaseValidationCommand => databaseValidationCommand ??= new CommandHandler(DatabaseValidation);
        private ICommand databaseValidationCommand;

        public ICommand CopyInvalidFileListCommand => copyInvalidFileListCommand ??= new CommandHandler(CopyInvalidFileList);
        private ICommand copyInvalidFileListCommand;

        public ICommand FileFinderCommand => fileFinderCommand ??= new CommandHandler(FileFinder);
        private ICommand fileFinderCommand;

        public ICommand CopyFileFinderResultCommand => copyFileFinderResultCommand ??= new CommandHandler(CopyFileFinderResult);
        private ICommand copyFileFinderResultCommand;

        public ICommand RemoveFileFinderResultCommand => removeFileFinderResultCommand ??= new CommandHandler(RemoveFileFinderResult);
        private ICommand removeFileFinderResultCommand;

        public string BackupResult
        {
            get => backupResult;
            set => SetProperty(ref backupResult, value);
        }
        private string backupResult;

        public ObservableCollection<BackupFile> BackupFiles { get; } = new();

        public string DatabaseValidationResult
        {
            get => databaseValidationResult;
            set => SetProperty(ref databaseValidationResult, value);
        }
        private string databaseValidationResult = "Not validated.";

        public ObservableCollection<string> DabaseValidationErrors { get; } = new();

        public string InvalidFileList
        {
            get => invalidFileList;
            set => SetProperty(ref invalidFileList, value);
        }
        private string invalidFileList = string.Empty;

        public string FileFinderResult
        {
            get => fileFinderResult;
            set => SetProperty(ref fileFinderResult, value);
        }
        private string fileFinderResult = "Not executed.";

        public string MissingFilesList
        {
            get => missingFilesList;
            set => SetProperty(ref missingFilesList, value);
        }
        private string missingFilesList = string.Empty;

        private readonly Model.Model model = Model.Model.Instance;

        public ToolsViewModel()
        {
            ScanBackupFiles();
        }

        private void CreateBackup()
        {
            try
            {
                new DatabaseBackup().CreateBackup();
                ScanBackupFiles();
            }
            catch (IOException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }

        private void ScanBackupFiles()
        {
            BackupFiles.Clear();

            var backupHandler = new DatabaseBackup();

            if (Directory.Exists(backupHandler.BackupDirectory))
            {
                foreach (var backupFile in backupHandler.ListAvailableBackupFiles())
                {
                    BackupFiles.Add(backupFile);
                }

                BackupResult = BackupFiles.Count > 0 ? $"{BackupFiles.Count} database backup files found:" : $"No database backup files found!";
            }
            else
            {
                BackupResult = "Directory for configured database does not exist.";
            }

            OnPropertyChanged(nameof(BackupResult));
        }

        private void DatabaseValidation()
        {
            DabaseValidationErrors.Clear();

            var filesValidator = new FilesModelValidator();
            List<FilesModel> invalidFiles = new();
            foreach (var file in model.DbAccess.GetFiles())
            {
                var result = filesValidator.Validate(file);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"File {file.Id}: {error.ErrorMessage}");
                    }
                    invalidFiles.Add(file);
                }
            }
            InvalidFileList = Utils.CreateFileList(invalidFiles);

            var personValidator = new PersonModelValidator();
            foreach (var person in model.DbAccess.GetPersons())
            {
                var result = personValidator.Validate(person);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Person {person.Id}: {error.ErrorMessage}");
                    }
                }
            }

            var locationValidator = new LocationModelValidator();
            foreach (var location in model.DbAccess.GetLocations())
            {
                var result = locationValidator.Validate(location);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Location {location.Id}: {error.ErrorMessage}");
                    }
                }
            }

            var tagValidator = new TagModelValidator();
            foreach (var tag in model.DbAccess.GetTags())
            {
                var result = tagValidator.Validate(tag);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Tag {tag.Id}: {error.ErrorMessage}");
                    }
                }
            }

            DatabaseValidationResult = DabaseValidationErrors.Count > 0 ? $"{DabaseValidationErrors.Count} errors found:" : $"No errors found.";
            OnPropertyChanged(nameof(DabaseValidationErrors));
        }

        private void CopyInvalidFileList()
        {
            ClipboardService.SetText(InvalidFileList);
        }

        private void FileFinder()
        {
            FileFinderResult = "Running, please wait...";

            List<FilesModel> missingFiles = new();
            foreach (var file in model.DbAccess.GetFiles())
            {
                var fileAbsPath = model.FilesystemAccess.ToAbsolutePath(file.Path);
                if (!File.Exists(fileAbsPath))
                {
                    missingFiles.Add(file);
                }
            }

            FileFinderResult = missingFiles.Count == 0 ? "No missing files found." : $"{missingFiles.Count} meta-data for missing files found.";
            MissingFilesList = Utils.CreateFileList(missingFiles);
        }

        private void CopyFileFinderResult()
        {
            ClipboardService.SetText(MissingFilesList);
        }

        private void RemoveFileFinderResult()
        {
            var fileIds = Utils.CreateFileIds(MissingFilesList);
            if (Utils.ShowConfirmDialog($"Remove {fileIds.Count} meta-data for missing files?"))
            {
                foreach (var fileId in fileIds)
                {
                    model.DbAccess.DeleteFile(fileId);
                }
            }
        }
    }
}
