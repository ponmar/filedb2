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

        public StartViewModel(FileDB2BrowserConfig browserConfig)
        {
            Database = browserConfig.Database;
            FilesRootDirectory = browserConfig.FilesRootDirectory;
            SlideshowDelay = browserConfig.SlideshowDelay.TotalSeconds.ToString();
            SearchHistorySize = browserConfig.SearchHistorySize.ToString();
        }
    }
}
