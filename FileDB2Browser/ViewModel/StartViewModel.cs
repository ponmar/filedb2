using System.Windows.Input;
using FileDB2Browser.Config;

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
            private set { SetProperty(ref filesRootDirectory, value); }
        }
        private string filesRootDirectory;

        public string SlideshowDelay
        {
            get => slideshowDelay;
            private set { SetProperty(ref slideshowDelay, value); }
        }
        private string slideshowDelay;

        public string SearchHistorySize
        {
            get => searchHistorySize;
            private set { SetProperty(ref searchHistorySize, value); }
        }
        private string searchHistorySize;

        public ICommand ResetConfigurationCommand => resetConfigurationCommand ??= new CommandHandler(ResetConfiguration);
        private ICommand resetConfigurationCommand;

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
    }
}
