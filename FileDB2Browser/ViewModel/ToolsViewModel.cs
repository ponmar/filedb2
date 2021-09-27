using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class BackupFile
    {
        public string Filename { get; }

        public BackupFile(string filename)
        {
            Filename = filename;
        }
    }

    public class ToolsViewModel : ViewModelBase
    {
        public ICommand CreateBackupCommand => createBackupCommand ??= new CommandHandler(CreateBackup);
        private ICommand createBackupCommand;

        public ObservableCollection<BackupFile> BackupFiles { get; } = new();

        public ToolsViewModel()
        {
            ScanBackupFiles();
        }

        public void CreateBackup(object parameter)
        {
            var db = Utils.BrowserConfig.Database;
            if (File.Exists(db))
            {
                var directoryPath = Path.GetDirectoryName(db);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                var backupFilename = $"backup_{timestamp}.db";
                var backupFilePath = Path.Combine(directoryPath, backupFilename);
                if (!File.Exists(backupFilePath))
                {
                    File.Copy(db, backupFilePath);
                    ScanBackupFiles();
                }
            }
        }

        private void ScanBackupFiles()
        {
            var backupDir = Path.GetDirectoryName(Utils.BrowserConfig.Database);
            if (Directory.Exists(backupDir))
            {
                BackupFiles.Clear();
                foreach (var backupFile in Directory.GetFiles(backupDir, "backup_*.db"))
                {
                    BackupFiles.Add(new BackupFile(backupFile));
                }
            }
        }
    }
}
