using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Browser.Config;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class NewFile
    {
        public string Path { get; set; }

        public string DateModified { get; set; }
    }

    public class ImportViewModel
    {
        public ICommand ScanNewFilesCommand
        {
            get
            {
                return scanNewFilesCommand ??= new CommandHandler(ScanNewFiles);
            }
        }
        private ICommand scanNewFilesCommand;

        private readonly FileDB2Handle fileDB2Handle;
        private readonly FileDB2BrowserConfig browserConfig;

        public ObservableCollection<NewFile> NewFiles { get; } = new ObservableCollection<NewFile>();

        public ImportViewModel(FileDB2Handle fileDB2Handle, FileDB2BrowserConfig browserConfig)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.browserConfig = browserConfig;
        }

        public void ScanNewFiles(object parameter)
        {
            NewFiles.Clear();
            var newFiles = fileDB2Handle.ListNewFilesystemFiles(browserConfig.BlacklistedFilePathPatterns, browserConfig.WhitelistedFilePathPatterns, browserConfig.IncludeHiddenDirectories).Select(p => new NewFile()
            {
                Path = p,
                DateModified = GetDateModified(p),
            });

            foreach (var newFile in newFiles)
            {
                NewFiles.Add(newFile);
            }
        }

        private string GetDateModified(string internalPath)
        {
            var path = fileDB2Handle.InternalPathToPath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
