using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDBInterface;

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

        public ObservableCollection<BackupFile> BackupFiles { get; } = new();

        public ToolsViewModel()
        {
            ScanBackupFiles();
            ShowBackupReminder();
        }

        public void CreateBackup()
        {
            var db = Utils.BrowserConfig.Database;
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
            if (Utils.BrowserConfig.StartupBackupReminderAfterDays <= 0)
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
                if (latestBackupDaysAge >= Utils.BrowserConfig.StartupBackupReminderAfterDays)
                {
                    Utils.ShowWarningDialog($"Backup reminder: Last database backup created {latestBackupDaysAge} days ago!");
                }
            }
        }

        private void ScanBackupFiles()
        {
            var backupDir = Path.GetDirectoryName(Utils.BrowserConfig.Database);
            if (Directory.Exists(backupDir))
            {
                BackupFiles.Clear();
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
            }
        }
    }
}
