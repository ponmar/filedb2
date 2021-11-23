using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDB.Config;
using FileDB.Validators;
using FileDBInterface;
using FileDBInterface.Exceptions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace FileDB.ViewModel
{
     public class SettingsViewModel : ViewModelBase
    {
        public string ConfigName
        {
            get => configName;
            set => SetProperty(ref configName, value);
        }
        private string configName;

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

        public bool BirthdayReminder
        {
            get => birthdayReminder;
            set => SetProperty(ref birthdayReminder, value);
        }
        private bool birthdayReminder;

        public bool RipReminder
        {
            get => ripReminder;
            set => SetProperty(ref ripReminder, value);
        }
        private bool ripReminder;

        public string LocationLink
        {
            get => locationLink;
            set => SetProperty(ref locationLink, value);
        }
        private string locationLink;

        public string FileToLocationMaxDistance
        {
            get => fileToLocationMaxDistance;
            set => SetProperty(ref fileToLocationMaxDistance, value);
        }
        private string fileToLocationMaxDistance;

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

        public ICommand SetDefaultSlideshowDelayCommand => setDefaultSlideshowDelayCommand ??= new CommandHandler(x => SlideshowDelay = DefaultConfigs.Default.SlideshowDelay.ToString());
        private ICommand setDefaultSlideshowDelayCommand;

        public ICommand SetDefaultStartupBackupReminderAfterDaysCommand => setDefaultStartupBackupReminderAfterDaysCommand ??= new CommandHandler(x => StartupBackupReminderAfterDays = DefaultConfigs.Default.StartupBackupReminderAfterDays.ToString());
        private ICommand setDefaultStartupBackupReminderAfterDaysCommand;

        public ICommand SetDefaultSearchHistorySizeCommand => setDefaultSearchHistorySizeCommand ??= new CommandHandler(x => SearchHistorySize = DefaultConfigs.Default.SearchHistorySize.ToString());
        private ICommand setDefaultSearchHistorySizeCommand;

        public ICommand SetDefaultBlacklistedFilePathPatternsJsonCommand => setDefaultBlacklistedFilePathPatternsJsonCommand ??= new CommandHandler(x => BlacklistedFilePathPatternsJson = JsonConvert.SerializeObject(DefaultConfigs.Default.BlacklistedFilePathPatterns));
        private ICommand setDefaultBlacklistedFilePathPatternsJsonCommand;

        public ICommand SetDefaultWhitelistedFilePathPatternsJsonCommand => setDefaultWhitelistedFilePathPatternsJsonCommand ??= new CommandHandler(x => WhitelistedFilePathPatternsJson = JsonConvert.SerializeObject(DefaultConfigs.Default.WhitelistedFilePathPatterns));
        private ICommand setDefaultWhitelistedFilePathPatternsJsonCommand;

        public ICommand SetDefaultIncludeHiddenDirectoriesCommand => setDefaultIncludeHiddenDirectoriesCommand ??= new CommandHandler(x => IncludeHiddenDirectories = DefaultConfigs.Default.IncludeHiddenDirectories);
        private ICommand setDefaultIncludeHiddenDirectoriesCommand;

        public ICommand SetDefaultReadOnlyCommand => setDefaultReadOnlyCommand ??= new CommandHandler(x => ReadOnly = DefaultConfigs.Default.ReadOnly);
        private ICommand setDefaultReadOnlyCommand;

        public ICommand SetDefaultBirthdayReminderCommand => setDefaultBirthdayReminderCommand ??= new CommandHandler(x => BirthdayReminder = DefaultConfigs.Default.BirthdayReminder);
        private ICommand setDefaultBirthdayReminderCommand;

        public ICommand SetDefaultRipReminderCommand => setDefaultRipReminderCommand ??= new CommandHandler(x => RipReminder = DefaultConfigs.Default.RipReminder);
        private ICommand setDefaultRipReminderCommand;

        public ICommand SetDefaultLocationLinkCommand => setDefaultLocationLinkCommand ??= new CommandHandler(x => LocationLink = DefaultConfigs.Default.LocationLink);
        private ICommand setDefaultLocationLinkCommand;

        public ICommand SetDefaultFileToLocationMaxDistanceCommand => setDefaultFileToLocationMaxDistanceCommand ??= new CommandHandler(x => FileToLocationMaxDistance = DefaultConfigs.Default.FileToLocationMaxDistance.ToString());
        private ICommand setDefaultFileToLocationMaxDistanceCommand;

        public SettingsViewModel()
        {
            UpdateFromConfiguration();
        }

        private void UpdateFromConfiguration()
        {
            ConfigName = Utils.Config.Name;
            Database = Utils.Config.Database;
            FilesRootDirectory = Utils.Config.FilesRootDirectory;
            SlideshowDelay = Utils.Config.SlideshowDelay.ToString();
            SearchHistorySize = Utils.Config.SearchHistorySize.ToString();
            IncludeHiddenDirectories = Utils.Config.IncludeHiddenDirectories;
            BlacklistedFilePathPatternsJson = JsonConvert.SerializeObject(Utils.Config.BlacklistedFilePathPatterns);
            WhitelistedFilePathPatternsJson = JsonConvert.SerializeObject(Utils.Config.WhitelistedFilePathPatterns);
            ReadOnly = Utils.Config.ReadOnly;
            StartupBackupReminderAfterDays = Utils.Config.StartupBackupReminderAfterDays.ToString();
            BirthdayReminder = Utils.Config.BirthdayReminder;
            RipReminder = Utils.Config.RipReminder;
            LocationLink = Utils.Config.LocationLink;
            FileToLocationMaxDistance = Utils.Config.FileToLocationMaxDistance.ToString();
        }

        public void ResetConfiguration()
        {
            UpdateFromConfiguration();
        }

        public void SaveConfiguration()
        {
            if (!int.TryParse(SearchHistorySize, out int searchHistorySize))
            {
                Utils.ShowErrorDialog("Invalid search history size");
                return;
            }

            if (!int.TryParse(SlideshowDelay, out int slideshowDelay))
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

            if (!int.TryParse(StartupBackupReminderAfterDays, out int startupBackupReminderAfterDays))
            {
                Utils.ShowErrorDialog("Invalid startup backup reminder interval");
                return;
            }

            if (!double.TryParse(FileToLocationMaxDistance, out var fileToLocationMaxDistance))
            {
                Utils.ShowErrorDialog("Invalid file to location max distance");
                return;
            }

            var config = new Config.Config(
                ConfigName,
                Database,
                FilesRootDirectory,
                fileToLocationMaxDistance,
                blacklistedFilePathPatterns,
                whitelistedFilePathPatterns,
                IncludeHiddenDirectories,
                slideshowDelay,
                searchHistorySize,
                ReadOnly,
                startupBackupReminderAfterDays,
                BirthdayReminder,
                RipReminder,
                LocationLink);

            var result = new ConfigValidator().Validate(config);
            if (!result.IsValid)
            {
                Utils.ShowErrorDialog(result.Errors.Select(x => x.ErrorMessage));
                return;
            }

            if (!Utils.ShowConfirmDialog($"Write your configuration to {Utils.BrowserConfigIO.FilePath}?"))
            {
                return;
            }

            if (Utils.BrowserConfigIO.Write(config))
            {
                Utils.Config = config;
                Utils.ReloadFileDBHandle();
                Utils.ShowInfoDialog("Configuration saved. Restart to enable all settings.");
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
                Filter = $"{Utils.ApplicationName} database files (*.db)|*.db",
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
                    DatabaseUtils.CreateDatabase(Database);
                }
                catch (DatabaseWrapperException e)
                {
                    Utils.ShowErrorDialog(e.Message);
                }
            }
        }

        public bool CreateDatabasePossible()
        {
            return !File.Exists(Database);
        }
    }
}
