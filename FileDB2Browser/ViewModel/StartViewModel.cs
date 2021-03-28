using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Browser.Config;
using FileDB2Interface;

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
        string slideshowDelay;

        public StartViewModel(FileDB2BrowserConfig browserConfig)
        {
            Database = browserConfig.Database;
            FilesRootDirectory = browserConfig.FilesRootDirectory;
            SlideshowDelay = browserConfig.SlideshowDelay.TotalSeconds.ToString();
        }
    }
}
