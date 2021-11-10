using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDB2Browser.Config;
using FileDB2Interface;
using FileDB2Interface.Exceptions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace FileDB2Browser.ViewModel
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

    public class SettingsViewModel : ViewModelBase
    {
        private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

        public string Database
        {
            get => database;
            set => SetProperty(ref database, value);
        }
        private string database;

        public string FilesRootDirectory
        {
            get => filesRootDirectory;
            set => SetProperty(ref filesRootDirectory, value);
        }
        private string filesRootDirectory;

        public string SlideshowDelay
        {
            get => slideshowDelay;
            set => SetProperty(ref slideshowDelay, value);
        }
        private string slideshowDelay;

        public string SearchHistorySize
        {
            get => searchHistorySize;
            set => SetProperty(ref searchHistorySize, value);
        }
        private string searchHistorySize;

        public bool IncludeHiddenDirectories
        {
            get => includeHiddenDirectories;
            set => SetProperty(ref includeHiddenDirectories, value);
        }
        private bool includeHiddenDirectories;

        public string BlacklistedFilePathPatternsJson
        {
            get => blacklistedFilePathPatternsJson;
            set => SetProperty(ref blacklistedFilePathPatternsJson, value);
        }
        private string blacklistedFilePathPatternsJson;

        public string WhitelistedFilePathPatternsJson
        {
            get => whitelistedFilePathPatternsJson;
            set => SetProperty(ref whitelistedFilePathPatternsJson, value);
        }
        private string whitelistedFilePathPatternsJson;

        public bool ReadOnly
        {
            get => readOnly;
            set => SetProperty(ref readOnly, value);
        }
        private bool readOnly;

        public string StartupBackupReminderAfterDays
        {
            get => startupBackupReminderAfterDays;
            set => SetProperty(ref startupBackupReminderAfterDays, value);
        }
        private string startupBackupReminderAfterDays;

        public ICommand ResetConfigurationCommand => resetConfigurationCommand ??= new CommandHandler(ResetConfiguration);
        private ICommand resetConfigurationCommand;

        public ICommand SaveConfigurationCommand => saveConfigurationCommand ??= new CommandHandler(SaveConfiguration);
        private ICommand saveConfigurationCommand;

        public ICommand BrowseDatabaseCommand => browseDatabaseCommand ??= new CommandHandler(BrowseDatabase);
        private ICommand browseDatabaseCommand;

        public ICommand BrowseFilesRootDirectoryCommand => browseFilesRootDirectoryCommand ??= new CommandHandler(BrowseFilesRootDirectory);
        private ICommand browseFilesRootDirectoryCommand;

        public ICommand CreateDatabaseCommand => createDatabaseCommand ??= new CommandHandler(CreateDatabase, CreateDatabasePossible);
        private ICommand createDatabaseCommand;

        public ICommand CreateBackupCommand => createBackupCommand ??= new CommandHandler(CreateBackup);
        private ICommand createBackupCommand;

        public ObservableCollection<BackupFile> BackupFiles { get; } = new();

        public SettingsViewModel()
        {
            Init();
            ScanBackupFiles();
            ShowBackupReminder();
        }

        private void Init()
        {
            Database = Utils.BrowserConfig.Database;
            FilesRootDirectory = Utils.BrowserConfig.FilesRootDirectory;
            SlideshowDelay = Utils.BrowserConfig.SlideshowDelay.TotalSeconds.ToString();
            SearchHistorySize = Utils.BrowserConfig.SearchHistorySize.ToString();
            IncludeHiddenDirectories = Utils.BrowserConfig.IncludeHiddenDirectories;
            BlacklistedFilePathPatternsJson = JsonConvert.SerializeObject(Utils.BrowserConfig.BlacklistedFilePathPatterns);
            WhitelistedFilePathPatternsJson = JsonConvert.SerializeObject(Utils.BrowserConfig.WhitelistedFilePathPatterns);
            ReadOnly = Utils.BrowserConfig.ReadOnly;
            StartupBackupReminderAfterDays = Utils.BrowserConfig.StartupBackupReminderAfterDays.ToString();
        }

        public void ResetConfiguration()
        {
            if (Utils.BrowserConfigIO.Write(BrowserConfigFactory.GetDefault()))
            {
                Utils.BrowserConfig = BrowserConfigFactory.GetDefault();
                Utils.ReloadFileDB2Handle();
                Init();
            }
            else
            {
                Utils.ShowErrorDialog("Unable to reset configuration");
            }
        }

        public void SaveConfiguration()
        {
            if (!Database.EndsWith(".db"))
            {
                Utils.ShowErrorDialog("Invalid database filename");
                return;
            }
            if (!File.Exists(Database))
            {
                Utils.ShowErrorDialog("Database missing");
                return;
            }

            if (!int.TryParse(SearchHistorySize, out int searchHistorySize) || searchHistorySize < 0 || searchHistorySize > 10)
            {
                Utils.ShowErrorDialog("Invalid search history size");
                return;
            }

            if (!int.TryParse(SlideshowDelay, out int slideshowDelay) || slideshowDelay < 1)
            {
                Utils.ShowErrorDialog("Invalid slideshow delay");
                return;
            }

            List<string> blacklistedFilePathPatterns;
            try
            {
                blacklistedFilePathPatterns = JsonConvert.DeserializeObject<List<string>>(BlacklistedFilePathPatternsJson);
            }
            catch (JsonException)
            {
                Utils.ShowErrorDialog("Invalid blacklisted file path patterns");
                return;
            }

            List<string> whitelistedFilePathPatterns;
            try
            {
                whitelistedFilePathPatterns = JsonConvert.DeserializeObject<List<string>>(WhitelistedFilePathPatternsJson);
            }
            catch (JsonException)
            {
                Utils.ShowErrorDialog("Invalid whitelisted file path patterns");
                return;
            }

            if (!int.TryParse(StartupBackupReminderAfterDays, out int startupBackupReminderAfterDays) || startupBackupReminderAfterDays < 1)
            {
                Utils.ShowErrorDialog("Invalid startup backup reminder interval");
                return;
            }

            var config = new Config.Config(
                Database,
                FilesRootDirectory,
                blacklistedFilePathPatterns,
                whitelistedFilePathPatterns,
                IncludeHiddenDirectories,
                TimeSpan.FromSeconds(slideshowDelay),
                searchHistorySize,
                ReadOnly,
                startupBackupReminderAfterDays);

            if (Utils.BrowserConfigIO.Write(config))
            {
                Utils.BrowserConfig = config;
                Utils.ReloadFileDB2Handle();
                Utils.ShowInfoDialog("Configuration saved");
            }
            else
            {
                Utils.ShowErrorDialog("Unable to save configuration");
            }
        }

        public void BrowseDatabase()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "FileDB2 database files (*.db)|*.db",
                InitialDirectory = @"c:\", // TODO: get directory from current path? Or appdata?
            };
            if (fileDialog.ShowDialog() == true)
            {
                Database = fileDialog.FileName;
            }
        }

        public void BrowseFilesRootDirectory()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "Any file within files root directory (*.*)|*.*",
                InitialDirectory = @"c:\", // TODO: get directory from current path? Or appdata?
                ValidateNames = false,
                CheckFileExists = false,
            };
            if (fileDialog.ShowDialog() == true)
            {
                var dir = Path.GetDirectoryName(fileDialog.FileName);
                if (Directory.Exists(dir))
                {
                    FilesRootDirectory = dir;
                }
                else
                {
                    Utils.ShowErrorDialog($"No such directory: {dir}");
                }
            }
        }

        public void CreateDatabase()
        {
            if (Utils.ShowConfirmDialog($"Create database {Database}?"))
            {
                try
                {
                    FileDB2Utils.CreateDatabase(Database);
                }
                catch (FileDB2Exception e)
                {
                    Utils.ShowErrorDialog(e.Message);
                }
            }
        }

        public bool CreateDatabasePossible()
        {
            return !File.Exists(Database);
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
    }
}
