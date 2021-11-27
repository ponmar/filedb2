﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDBInterface.Validators;

namespace FileDB.ViewModel
{
    public class BackupFile
    {
        public string Filename { get; }

        public DateTime Timestamp { get; }

        public TimeSpan Age => DateTime.Now - Timestamp;

        public BackupFile(string filename, DateTime timestamp)
        {
            Filename = filename;
            Timestamp = timestamp;
        }
    }

    public class ToolsViewModel : ViewModelBase
    {
        private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

        public ICommand CreateBackupCommand => createBackupCommand ??= new CommandHandler(CreateBackup);
        private ICommand createBackupCommand;

        public ICommand DatabaseValidationCommand => databaseValidationCommand ??= new CommandHandler(DatabaseValidation);
        private ICommand databaseValidationCommand;

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

        public ToolsViewModel()
        {
            ScanBackupFiles();
            ShowBackupReminder();
        }

        private void CreateBackup()
        {
            var db = Utils.Config.Database;
            if (File.Exists(db))
            {
                var directoryPath = Path.GetDirectoryName(db);
                var timestamp = DateTime.Now.ToString(BackupFileTimestampFormat);
                var backupFilename = $"backup_{timestamp}.db";
                var backupFilePath = Path.Combine(directoryPath, backupFilename);
                if (!File.Exists(backupFilePath))
                {
                    File.Copy(db, backupFilePath);
                    ScanBackupFiles();
                }
                else
                {
                    Utils.ShowErrorDialog($"Backup file already available");
                }
            }
            else
            {
                Utils.ShowErrorDialog($"Missing database: {db}");
            }
        }

        private void ShowBackupReminder()
        {
            if (Utils.Config.StartupBackupReminderAfterDays <= 0)
            {
                return;
            }

            if (BackupFiles.Count == 0)
            {
                Utils.ShowWarningDialog("Backup reminder: No database backup has been created!");
            }
            else
            {
                var latestBackupDaysAge = (int)BackupFiles.Min(x => x.Age).TotalDays;
                if (latestBackupDaysAge >= Utils.Config.StartupBackupReminderAfterDays)
                {
                    Utils.ShowWarningDialog($"Backup reminder: Last database backup created {latestBackupDaysAge} days ago!");
                }
            }
        }

        private void ScanBackupFiles()
        {
            BackupFiles.Clear();

            var backupDir = Path.GetDirectoryName(Utils.Config.Database);
            if (Directory.Exists(backupDir))
            {
                foreach (var filePath in Directory.GetFiles(backupDir, "backup_*.db"))
                {
                    var filenameParts = filePath.Split("_");
                    if (filenameParts.Length >= 2)
                    {
                        var timestampString = filenameParts[filenameParts.Length - 1].Replace(".db", "");
                        try
                        {
                            var timestamp = DateTime.ParseExact(timestampString, BackupFileTimestampFormat, null);
                            BackupFiles.Add(new BackupFile(filePath, timestamp));
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }

                BackupResult = BackupFiles.Count > 0 ? $"{BackupFiles.Count} database backup files found:" : $"No database backup files found!";
            }
            else
            {
                BackupResult = "Directory for configured database does not exist.";
            }
        }

        private void DatabaseValidation()
        {
            DabaseValidationErrors.Clear();

            var filesValidator = new FilesModelValidator();
            foreach (var file in Utils.DatabaseWrapper.GetFiles())
            {
                var result = filesValidator.Validate(file);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"File {file.id}: {error.ErrorMessage}");
                    }
                }
            }

            var personValidator = new PersonModelValidator();
            foreach (var person in Utils.DatabaseWrapper.GetPersons())
            {
                var result = personValidator.Validate(person);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Person {person.id}: {error.ErrorMessage}");
                    }
                }
            }

            var locationValidator = new LocationModelValidator();
            foreach (var location in Utils.DatabaseWrapper.GetLocations())
            {
                var result = locationValidator.Validate(location);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Location {location.id}: {error.ErrorMessage}");
                    }
                }
            }

            var tagValidator = new TagModelValidator();
            foreach (var tag in Utils.DatabaseWrapper.GetTags())
            {
                var result = tagValidator.Validate(tag);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        DabaseValidationErrors.Add($"Tag {tag.id}: {error.ErrorMessage}");
                    }
                }
            }

            DatabaseValidationResult = DabaseValidationErrors.Count > 0 ? $"{DabaseValidationErrors.Count} errors found:" : $"No errors found.";
        }
    }
}
