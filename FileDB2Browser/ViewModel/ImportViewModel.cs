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
using FileDB2Interface.Exceptions;

namespace FileDB2Browser.ViewModel
{
    public class NewFile
    {
        public string Path { get; set; }

        public string DateModified { get; set; }
    }

    public class ImportViewModel : ViewModelBase
    {
        public ICommand ScanNewFilesCommand => scanNewFilesCommand ??= new CommandHandler(ScanNewFiles);
        private ICommand scanNewFilesCommand;

        public ICommand ImportNewFilesCommand => importNewFilesCommand ??= new CommandHandler(ImportNewFiles, CanImportNewFiles);
        private ICommand importNewFilesCommand;

        public ObservableCollection<NewFile> NewFiles { get; } = new ObservableCollection<NewFile>();

        public ImportViewModel()
        {
        }

        public void ScanNewFiles()
        {
            NewFiles.Clear();
            var newFiles = Utils.FileDB2Handle.ListNewFilesystemFiles(Utils.BrowserConfig.BlacklistedFilePathPatterns, Utils.BrowserConfig.WhitelistedFilePathPatterns, Utils.BrowserConfig.IncludeHiddenDirectories).Select(p => new NewFile()
            {
                Path = p,
                DateModified = GetDateModified(p),
            });

            foreach (var newFile in newFiles)
            {
                NewFiles.Add(newFile);
            }
        }

        public bool CanImportNewFiles()
        {
            return NewFiles.Count > 0;
        }

        public void ImportNewFiles()
        {
            try
            {
                foreach (var newFile in NewFiles)
                {
                    Utils.FileDB2Handle.InsertFile(newFile.Path);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }

            NewFiles.Clear();
        }

        private string GetDateModified(string internalPath)
        {
            var path = Utils.FileDB2Handle.InternalPathToPath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
