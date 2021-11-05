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

        public ICommand ImportNewFilesCommand => importNewFilesCommand ??= new CommandHandler(ImportNewFiles);
        private ICommand importNewFilesCommand;

        public ObservableCollection<NewFile> NewFiles { get; } = new ObservableCollection<NewFile>();

        public bool NewFilesAvailable => NewFiles.Count > 0;

        public ImportViewModel()
        {
        }

        public void ScanNewFiles()
        {
            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));

            // TODO: show counter?
            foreach (var internalFilePath in Utils.FileDB2Handle.ListNewFilesystemFiles(Utils.BrowserConfig.BlacklistedFilePathPatterns, Utils.BrowserConfig.WhitelistedFilePathPatterns, Utils.BrowserConfig.IncludeHiddenDirectories))
            {
                NewFiles.Add(new NewFile()
                {
                    Path = internalFilePath,
                    DateModified = GetDateModified(internalFilePath),
                });
            }

            OnPropertyChanged(nameof(NewFilesAvailable));
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
            OnPropertyChanged(nameof(NewFilesAvailable));
        }

        private string GetDateModified(string internalPath)
        {
            var path = Utils.FileDB2Handle.InternalPathToPath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
