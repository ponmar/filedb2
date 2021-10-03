using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        public BackupFile(string filename)
        {
            Filename = filename;
        }
    }

    public class SettingsViewModel : ViewModelBase
    {
        public string Database
        {
            get => database;
            set { SetProperty(ref database, value); }
        }
        private string database;

        public string FilesRootDirectory
        {
            get => filesRootDirectory;
            set { SetProperty(ref filesRootDirectory, value); }
        }
        private string filesRootDirectory;

        public string SlideshowDelay
        {
            get => slideshowDelay;
            set { SetProperty(ref slideshowDelay, value); }
        }
        private string slideshowDelay;

        public string SearchHistorySize
        {
            get => searchHistorySize;
            set { SetProperty(ref searchHistorySize, value); }
        }
        private string searchHistorySize;

        public bool IncludeHiddenDirectories
        {
            get => includeHiddenDirectories;
            set { SetProperty(ref includeHiddenDirectories, value); }
        }
        private bool includeHiddenDirectories;

        public string BlacklistedFilePathPatternsJson
        {
            get => blacklistedFilePathPatternsJson;
            set { SetProperty(ref blacklistedFilePathPatternsJson, value); }
        }
        private string blacklistedFilePathPatternsJson;

        public string WhitelistedFilePathPatternsJson
        {
            get => whitelistedFilePathPatternsJson;
            set { SetProperty(ref whitelistedFilePathPatternsJson, value); }
        }
        private string whitelistedFilePathPatternsJson;

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
        }

        public void ResetConfiguration(object parameter)
        {
            if (FileDB2BrowserConfigIO.ResetConfiguration())
            {
                Utils.BrowserConfig = new FileDB2BrowserConfig();
                Utils.ReloadFileDB2Handle();
                Init();
            }
            else
            {
                Utils.ShowErrorDialog("Unable to reset configuration");
            }
        }

        public void SaveConfiguration(object parameter)
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

            var config = new FileDB2BrowserConfig()
            {
                Database = Database,
                FilesRootDirectory = FilesRootDirectory,
                SearchHistorySize = searchHistorySize,
                SlideshowDelay = TimeSpan.FromSeconds(slideshowDelay),
                IncludeHiddenDirectories = IncludeHiddenDirectories,
                BlacklistedFilePathPatterns = blacklistedFilePathPatterns,
                WhitelistedFilePathPatterns = whitelistedFilePathPatterns,
            };

            if (FileDB2BrowserConfigIO.Write(config))
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

        public void BrowseDatabase(object parameter)
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

        public void BrowseFilesRootDirectory(object parameter)
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

        public void CreateDatabase(object parameter)
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
                foreach (var backupFile in Directory.GetFiles(backupDir, "backup_*.db"))
                {
                    BackupFiles.Add(new BackupFile(backupFile));
                }
            }
        }
    }
}
