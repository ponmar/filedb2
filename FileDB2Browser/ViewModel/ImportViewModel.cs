using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class NewFile
    {
        public string Path { get; set; }
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

        public List<NewFile> NewFiles;

        public ImportViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
        }

        public void ScanNewFiles(object parameter)
        {
            NewFiles = fileDB2Handle.ListNewFilesystemFiles().Select(p => new NewFile() { Path = p }).ToList();
        }
    }
}
