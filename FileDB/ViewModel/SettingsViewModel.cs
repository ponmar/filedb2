﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using FileDB.Configuration;
using FileDB.Sorters;
using FileDB.Validators;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using Microsoft.Win32;

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

        public int SlideshowDelay
        {
            get => slideshowDelay;
            set => SetProperty(ref slideshowDelay, value);
        }
        private int slideshowDelay;

        public int SearchHistorySize
        {
            get => searchHistorySize;
            set => SetProperty(ref searchHistorySize, value);
        }
        private int searchHistorySize;

        public SortMethod DefaultSortMethod
        {
            get => defaultSortMethod;
            set => SetProperty(ref defaultSortMethod, value);
        }
        private SortMethod defaultSortMethod;

        public List<SortMethodDescription> SortMethods => Utils.GetSortMethods();

        public bool KeepSelectionAfterSort
        {
            get => keepSelectionAfterSort;
            set => SetProperty(ref keepSelectionAfterSort, value);
        }
        private bool keepSelectionAfterSort;

        public bool IncludeHiddenDirectories
        {
            get => includeHiddenDirectories;
            set => SetProperty(ref includeHiddenDirectories, value);
        }
        private bool includeHiddenDirectories;

        public string BlacklistedFilePathPatterns
        {
            get => blacklistedFilePathPatterns;
            set => SetProperty(ref blacklistedFilePathPatterns, value);
        }
        private string blacklistedFilePathPatterns;

        public string WhitelistedFilePathPatterns
        {
            get => whitelistedFilePathPatterns;
            set => SetProperty(ref whitelistedFilePathPatterns, value);
        }
        private string whitelistedFilePathPatterns;

        public bool ReadOnly
        {
            get => readOnly;
            set => SetProperty(ref readOnly, value);
        }
        private bool readOnly;

        public bool BackupReminder
        {
            get => backupReminder;
            set => SetProperty(ref backupReminder, value);
        }
        private bool backupReminder;

        public bool BirthdayReminder
        {
            get => birthdayReminder;
            set => SetProperty(ref birthdayReminder, value);
        }
        private bool birthdayReminder;

        public bool BirthdayReminderForDeceased
        {
            get => birthdayReminderForDeceased;
            set => SetProperty(ref birthdayReminderForDeceased, value);
        }
        private bool birthdayReminderForDeceased;

        public bool RipReminder
        {
            get => ripReminder;
            set => SetProperty(ref ripReminder, value);
        }
        private bool ripReminder;

        public bool MissingFilesRootDirNotification
        {
            get => missingFilesRootDirNotification;
            set => SetProperty(ref missingFilesRootDirNotification, value);
        }
        private bool missingFilesRootDirNotification;

        public string LocationLink
        {
            get => locationLink;
            set => SetProperty(ref locationLink, value);
        }
        private string locationLink;

        public int FileToLocationMaxDistance
        {
            get => fileToLocationMaxDistance;
            set => SetProperty(ref fileToLocationMaxDistance, value);
        }
        private int fileToLocationMaxDistance;

        public WindowMode WindowMode
        {
            get => windowMode;
            set => SetProperty(ref windowMode, value);
        }
        private WindowMode windowMode;

        public int CastHttpServerPort
        {
            get => castHttpServerPort;
            set => SetProperty(ref castHttpServerPort, value);
        }
        private int castHttpServerPort;

        public bool CacheFiles
        {
            get => cacheFiles;
            set => SetProperty(ref cacheFiles, value);
        }
        private bool cacheFiles;

        public List<WindowModeDescription> WindowModes => Utils.GetWindowModes();

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

        public ICommand SetDefaultSlideshowDelayCommand => setDefaultSlideshowDelayCommand ??= new CommandHandler(x => SlideshowDelay = DefaultConfigs.Default.SlideshowDelay);
        private ICommand setDefaultSlideshowDelayCommand;

        public ICommand SetDefaultBackupReminderCommand => setDefaultBackupReminderCommand ??= new CommandHandler(x => BackupReminder = DefaultConfigs.Default.BackupReminder);
        private ICommand setDefaultBackupReminderCommand;

        public ICommand SetDefaultSearchHistorySizeCommand => setDefaultSearchHistorySizeCommand ??= new CommandHandler(x => SearchHistorySize = DefaultConfigs.Default.SearchHistorySize);
        private ICommand setDefaultSearchHistorySizeCommand;

        public ICommand SetDefaultDefaultSortMethodCommand => setDefaultDefaultSortMethodCommand ??= new CommandHandler(x => DefaultSortMethod = DefaultConfigs.Default.DefaultSortMethod);
        private ICommand setDefaultDefaultSortMethodCommand;

        public ICommand SetDefaultCastHttpServerPortCommand => setDefaultCastHttpServerPortCommand ??= new CommandHandler(x => CastHttpServerPort = DefaultConfigs.Default.CastHttpServerPort);
        private ICommand setDefaultCastHttpServerPortCommand;

        public ICommand SetDefaultKeepSelectionAfterSortCommand => setDefaultKeepSelectionAfterSortCommand ??= new CommandHandler(x => KeepSelectionAfterSort = DefaultConfigs.Default.KeepSelectionAfterSort);
        private ICommand setDefaultKeepSelectionAfterSortCommand;

        public ICommand SetDefaultBlacklistedFilePathPatternsCommand => setDefaultBlacklistedFilePathPatternsCommand ??= new CommandHandler(x => BlacklistedFilePathPatterns = DefaultConfigs.Default.BlacklistedFilePathPatterns);
        private ICommand setDefaultBlacklistedFilePathPatternsCommand;

        public ICommand SetDefaultWhitelistedFilePathPatternsCommand => setDefaultWhitelistedFilePathPatternsCommand ??= new CommandHandler(x => WhitelistedFilePathPatterns = DefaultConfigs.Default.WhitelistedFilePathPatterns);
        private ICommand setDefaultWhitelistedFilePathPatternsCommand;

        public ICommand SetDefaultIncludeHiddenDirectoriesCommand => setDefaultIncludeHiddenDirectoriesCommand ??= new CommandHandler(x => IncludeHiddenDirectories = DefaultConfigs.Default.IncludeHiddenDirectories);
        private ICommand setDefaultIncludeHiddenDirectoriesCommand;

        public ICommand SetDefaultReadOnlyCommand => setDefaultReadOnlyCommand ??= new CommandHandler(x => ReadOnly = DefaultConfigs.Default.ReadOnly);
        private ICommand setDefaultReadOnlyCommand;

        public ICommand SetDefaultCacheFilesCommand => setDefaultCacheFilesCommand ??= new CommandHandler(x => CacheFiles = DefaultConfigs.Default.CacheFiles);
        private ICommand setDefaultCacheFilesCommand;

        public ICommand SetDefaultBirthdayReminderCommand => setDefaultBirthdayReminderCommand ??= new CommandHandler(x => BirthdayReminder = DefaultConfigs.Default.BirthdayReminder);
        private ICommand setDefaultBirthdayReminderCommand;

        public ICommand SetDefaultBirthdayReminderForDeceasedCommand => setDefaultBirthdayReminderForDeceasedCommand ??= new CommandHandler(x => BirthdayReminderForDeceased = DefaultConfigs.Default.BirthdayReminderForDeceased);
        private ICommand setDefaultBirthdayReminderForDeceasedCommand;

        public ICommand SetDefaultRipReminderCommand => setDefaultRipReminderCommand ??= new CommandHandler(x => RipReminder = DefaultConfigs.Default.RipReminder);
        private ICommand setDefaultRipReminderCommand;

        public ICommand SetDefaultMissingFilesRootDirNotificationCommand => setDefaultMissingFilesRootDirNotificationCommand ??= new CommandHandler(x => MissingFilesRootDirNotification = DefaultConfigs.Default.MissingFilesRootDirNotification);
        private ICommand setDefaultMissingFilesRootDirNotificationCommand;

        public ICommand SetDefaultLocationLinkCommand => setDefaultLocationLinkCommand ??= new CommandHandler(x => LocationLink = DefaultConfigs.Default.LocationLink);
        private ICommand setDefaultLocationLinkCommand;

        public ICommand SetDefaultFileToLocationMaxDistanceCommand => setDefaultFileToLocationMaxDistanceCommand ??= new CommandHandler(x => FileToLocationMaxDistance = DefaultConfigs.Default.FileToLocationMaxDistance);
        private ICommand setDefaultFileToLocationMaxDistanceCommand;

        public ICommand SetDefaultWindowModeCommand => setDefaultWindowModeCommand ??= new CommandHandler(x => WindowMode = DefaultConfigs.Default.WindowMode);
        private ICommand setDefaultWindowModeCommand;

        private readonly Model.Model model = Model.Model.Instance;

        public SettingsViewModel()
        {
            UpdateFromConfiguration();
        }

        private void UpdateFromConfiguration()
        {
            ConfigName = model.Config.Name;
            Database = model.Config.Database;
            FilesRootDirectory = model.Config.FilesRootDirectory;
            SlideshowDelay = model.Config.SlideshowDelay;
            SearchHistorySize = model.Config.SearchHistorySize;
            DefaultSortMethod = model.Config.DefaultSortMethod;
            KeepSelectionAfterSort = model.Config.KeepSelectionAfterSort;
            IncludeHiddenDirectories = model.Config.IncludeHiddenDirectories;
            BlacklistedFilePathPatterns = model.Config.BlacklistedFilePathPatterns;
            WhitelistedFilePathPatterns = model.Config.WhitelistedFilePathPatterns;
            ReadOnly = model.Config.ReadOnly;
            BackupReminder = model.Config.BackupReminder;
            BirthdayReminder = model.Config.BirthdayReminder;
            BirthdayReminderForDeceased = model.Config.BirthdayReminderForDeceased;
            RipReminder = model.Config.RipReminder;
            MissingFilesRootDirNotification = model.Config.MissingFilesRootDirNotification;
            LocationLink = model.Config.LocationLink;
            FileToLocationMaxDistance = model.Config.FileToLocationMaxDistance;
            WindowMode = model.Config.WindowMode;
            CastHttpServerPort = model.Config.CastHttpServerPort;
            CacheFiles = model.Config.CacheFiles;
        }

        public void ResetConfiguration()
        {
            UpdateFromConfiguration();
        }

        public void SaveConfiguration()
        {
            var config = new Config(
                ConfigName,
                Database,
                FilesRootDirectory,
                fileToLocationMaxDistance,
                BlacklistedFilePathPatterns,
                WhitelistedFilePathPatterns,
                IncludeHiddenDirectories,
                SlideshowDelay,
                SearchHistorySize,
                DefaultSortMethod,
                KeepSelectionAfterSort,
                ReadOnly,
                BackupReminder,
                BirthdayReminder,
                BirthdayReminderForDeceased,
                RipReminder,
                MissingFilesRootDirNotification,
                LocationLink,
                WindowMode,
                CastHttpServerPort,
                CacheFiles);

            var result = new ConfigValidator().Validate(config);
            if (!result.IsValid)
            {
                Dialogs.ShowErrorDialog(result);
                return;
            }

            var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName);

            if (!Dialogs.ShowConfirmDialog($"Write your configuration to {appDataConfig.FilePath}?"))
            {
                return;
            }

            if (appDataConfig.Write(config))
            {
                model.Config = config;
                model.ReloadHandles();
            }
            else
            {
                Dialogs.ShowErrorDialog("Unable to save configuration");
            }
        }

        public void BrowseDatabase()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = $"{Utils.ApplicationName} database files (*.db)|*.db",
                InitialDirectory = @"c:\",
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
                InitialDirectory = @"c:\",
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
                    Dialogs.ShowErrorDialog($"No such directory: {dir}");
                }
            }
        }

        public void CreateDatabase()
        {
            if (!CreateDatabasePossible())
            {
                Dialogs.ShowErrorDialog("No database filename specified");
                return;
            }

            if (Dialogs.ShowConfirmDialog($"Create database {Database}?"))
            {
                try
                {
                    DatabaseUtils.CreateDatabase(Database);
                }
                catch (DatabaseWrapperException e)
                {
                    Dialogs.ShowErrorDialog(e.Message);
                }
            }
        }

        public bool CreateDatabasePossible()
        {
            return !string.IsNullOrEmpty(Database) && !File.Exists(Database);
        }
    }
}
