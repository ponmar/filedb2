using System;
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
            // TODO: add more input validation

            if (!int.TryParse(SearchHistorySize, out int searchHistorySize))
            {
                Utils.ShowErrorDialog("Invalid search history size");
                return;
            }

            if (!int.TryParse(SearchHistorySize, out int slideshowDelay))
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
            /*
            var directoryDialog = new FolderBrowserDialog()
            {
                Filter = "*.db",
                InitialDirectory = @"c:\", // TODO: get directory from current path? Or appdata?
            };
            if (fileDialog.ShowDialog() == true)
            {
                Database = fileDialog.FileName;
            }
            */
        }
    }
}
