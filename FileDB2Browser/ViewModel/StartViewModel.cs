﻿using System;
using System.IO;
using System.Windows.Input;
using FileDB2Browser.Config;
using Microsoft.Win32;

namespace FileDB2Browser.ViewModel
{
    public class StartViewModel : ViewModelBase
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

        public ICommand ResetConfigurationCommand => resetConfigurationCommand ??= new CommandHandler(ResetConfiguration);
        private ICommand resetConfigurationCommand;

        public ICommand SaveConfigurationCommand => saveConfigurationCommand ??= new CommandHandler(SaveConfiguration);
        private ICommand saveConfigurationCommand;

        public ICommand BrowseDatabaseCommand => browseDatabaseCommand ??= new CommandHandler(BrowseDatabase);
        private ICommand browseDatabaseCommand;

        public ICommand BrowseFilesRootDirectoryCommand => browseFilesRootDirectoryCommand ??= new CommandHandler(BrowseFilesRootDirectory);
        private ICommand browseFilesRootDirectoryCommand;

        public StartViewModel()
        {
            Init();
        }

        private void Init()
        {
            Database = Utils.BrowserConfig.Database;
            FilesRootDirectory = Utils.BrowserConfig.FilesRootDirectory;
            SlideshowDelay = Utils.BrowserConfig.SlideshowDelay.TotalSeconds.ToString();
            SearchHistorySize = Utils.BrowserConfig.SearchHistorySize.ToString();
            IncludeHiddenDirectories = Utils.BrowserConfig.IncludeHiddenDirectories;
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
            if (!int.TryParse(SearchHistorySize, out int searchHistorySize) || searchHistorySize < 0 || searchHistorySize > 10)
            {
                Utils.ShowErrorDialog("Invalid search history size");
                return;
            }

            if (!int.TryParse(SearchHistorySize, out int slideshowDelay) || slideshowDelay < 1)
            {
                Utils.ShowErrorDialog("Invalid slideshow delay");
                return;
            }

            var config = new FileDB2BrowserConfig()
            {
                Database = Database,
                FilesRootDirectory = FilesRootDirectory,
                SearchHistorySize = searchHistorySize,
                SlideshowDelay = TimeSpan.FromSeconds(slideshowDelay),
                IncludeHiddenDirectories = IncludeHiddenDirectories,
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
    }
}
