﻿using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDBInterface;
using FileDBInterface.Exceptions;

namespace FileDB.ViewModel
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
            if (!Utils.ShowConfirmDialog($"Find all files, not yet imported, from directory {Utils.BrowserConfig.FilesRootDirectory}?"))
            {
                return;
            }

            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));

            // TODO: show counter?
            foreach (var internalFilePath in Utils.FileDBHandle.ListNewFilesystemFiles(Utils.BrowserConfig.BlacklistedFilePathPatterns, Utils.BrowserConfig.WhitelistedFilePathPatterns, Utils.BrowserConfig.IncludeHiddenDirectories))
            {
                NewFiles.Add(new NewFile()
                {
                    Path = internalFilePath,
                    DateModified = GetDateModified(internalFilePath),
                });
            }

            OnPropertyChanged(nameof(NewFilesAvailable));

            if (NewFiles.Count == 0)
            {
                Utils.ShowWarningDialog($"No new files found. Add files to directory {Utils.BrowserConfig.FilesRootDirectory} or configure another files root directory.");
            }
        }

        public void ImportNewFiles()
        {
            var locations = Utils.FileDBHandle.GetLocations();

            try
            {
                foreach (var newFile in NewFiles)
                {
                    Utils.FileDBHandle.InsertFile(newFile.Path);

                    var importedFile = Utils.FileDBHandle.GetFileByPath(newFile.Path);

                    if (importedFile.position != null && Utils.BrowserConfig.FileToLocationMaxDistance > 0.5)
                    {
                        var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.position).Value;

                        foreach (var locationWithPosition in locations.Where(x => x.position != null))
                        {
                            var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.position).Value;
                            var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                            if (distance < Utils.BrowserConfig.FileToLocationMaxDistance)
                            {
                                Utils.FileDBHandle.InsertFileLocation(importedFile.id, locationWithPosition.id);
                            }
                        }
                    }
                }
            }
            catch (DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }

            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));
        }

        private string GetDateModified(string internalPath)
        {
            var path = Utils.FileDBHandle.ToAbsolutePath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
