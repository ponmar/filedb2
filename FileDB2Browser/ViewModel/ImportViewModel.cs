﻿using System;
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

    public class ImportViewModel : ViewModelBase
    {
        public ICommand ScanNewFilesCommand
        {
            get
            {
                return scanNewFilesCommand ??= new CommandHandler(ScanNewFiles);
            }
        }
        private ICommand scanNewFilesCommand;

        public ICommand ImportNewFilesCommand
        {
            get
            {
                return importNewFilesCommand ??= new CommandHandler(ImportNewFiles, CanImportNewFiles);
            }
        }
        private ICommand importNewFilesCommand;

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

        public bool CanImportNewFiles()
        {
            return NewFiles.Count > 0;
        }

        public void ImportNewFiles(object parameter)
        {
            foreach (var newFile in NewFiles)
            {
                fileDB2Handle.InsertFile(newFile.Path);
            }

            NewFiles.Clear();
        }

        private string GetDateModified(string internalPath)
        {
            var path = fileDB2Handle.InternalPathToPath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
