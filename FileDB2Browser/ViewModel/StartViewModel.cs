using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class StartViewModel : ViewModelBase
    {
        public string Database { get; }

        public string FilesRootDirectory { get; }

        public StartViewModel(FileDB2Handle fileDB2Handle)
        {
            Database = fileDB2Handle.Config.Database;
            FilesRootDirectory = fileDB2Handle.Config.FilesRootDirectory;
        }
    }
}
