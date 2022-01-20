using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using TextCopy;

namespace FileDB.ViewModel
{
    public class NewFile
    {
        public string Path { get; set; }

        public string DateModified { get; set; }
    }

    public class ImportViewModel : ViewModelBase
    {
        public ICommand ScanNewFilesCommand => scanNewFilesCommand ??= new CommandHandler(ScanAllNewFiles);
        private ICommand scanNewFilesCommand;

        public ICommand ScanNewFilesInDirectoryCommand => scanNewFilesInDirectoryCommand ??= new CommandHandler(ScanNewFilesInDirectory);
        private ICommand scanNewFilesInDirectoryCommand;

        public ICommand ImportNewFilesCommand => importNewFilesCommand ??= new CommandHandler(ImportNewFiles);
        private ICommand importNewFilesCommand;

        public ICommand CopyImportedFileListCommand => copyImportedFileListCommand ??= new CommandHandler(CopyImportedFileList);
        private ICommand copyImportedFileListCommand;

        public ICommand RemoveFileListCommand => removeFileListCommand ??= new CommandHandler(RemoveFileListMethod);
        private ICommand removeFileListCommand;

        public string SubdirToScan
        {
            get => subdirToScan;
            set => SetProperty(ref subdirToScan, value);
        }
        private string subdirToScan;

        public ObservableCollection<NewFile> NewFiles { get; } = new();

        public bool NewFilesAvailable => NewFiles.Count > 0;

        public string ImportResult
        {
            get => importResult;
            set => SetProperty(ref importResult, value);
        }
        private string importResult = string.Empty;

        public string ImportedFileList
        {
            get => importedFileList;
            set => SetProperty(ref importedFileList, value);
        }
        private string importedFileList = string.Empty;

        public string RemoveFileList
        {
            get => removeFileList;
            set => SetProperty(ref removeFileList, value);
        }
        private string removeFileList;

        private readonly Model.Model model = Model.Model.Instance;

        public ImportViewModel()
        {
            SubdirToScan = model.Config.FilesRootDirectory;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object sender, EventArgs e)
        {
            SubdirToScan = model.Config.FilesRootDirectory;
        }

        public void ScanAllNewFiles()
        {
            ScanAllNewFiles(model.Config.FilesRootDirectory);
        }

        public void ScanNewFilesInDirectory()
        {
            if (string.IsNullOrEmpty(SubdirToScan))
            {
                Dialogs.ShowErrorDialog("No directory specified");
                return;
            }
            if (!Directory.Exists(SubdirToScan))
            {
                Dialogs.ShowErrorDialog("Specified directory does no exist");
                return;
            }
            if (!SubdirToScan.StartsWith(model.Config.FilesRootDirectory))
            {
                Dialogs.ShowErrorDialog($"Specified directory is not within the configured files root directory: {model.Config.FilesRootDirectory}");
                return;
            }
            ScanAllNewFiles(SubdirToScan);
        }

        public void ScanAllNewFiles(string pathToScan)
        {
            if (!Dialogs.ShowConfirmDialog($"Find all files, not yet imported, from '{pathToScan}'?"))
            {
                return;
            }

            NewFiles.Clear();
            ImportResult = string.Empty;
            ImportedFileList = string.Empty;
            OnPropertyChanged(nameof(NewFilesAvailable));

            var blacklistedFilePathPatterns = model.Config.BlacklistedFilePathPatterns.Split(";");
            var whitelistedFilePathPatterns = model.Config.WhitelistedFilePathPatterns.Split(";");

            foreach (var internalFilePath in model.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, model.Config.IncludeHiddenDirectories, model.DbAccess))
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
                Dialogs.ShowInfoDialog($"No new files found. Add your files to '{model.Config.FilesRootDirectory}'.");
            }
        }

        public void ImportNewFiles()
        {
            if (!Dialogs.ShowConfirmDialog($"Import meta-data from {NewFiles.Count} files?"))
            {
                return;
            }

            var locations = model.DbAccess.GetLocations();

            try
            {
                List<FilesModel> importedFiles = new();

                foreach (var newFile in NewFiles)
                {
                    model.DbAccess.InsertFile(newFile.Path, null, model.FilesystemAccess);

                    var importedFile = model.DbAccess.GetFileByPath(newFile.Path);

                    if (importedFile != null)
                    {
                        importedFiles.Add(importedFile);

                        if (importedFile.Position != null && model.Config.FileToLocationMaxDistance > 0.5)
                        {
                            var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position).Value;

                            foreach (var locationWithPosition in locations.Where(x => x.Position != null))
                            {
                                var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position).Value;
                                var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                                if (distance < model.Config.FileToLocationMaxDistance)
                                {
                                    model.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
                                }
                            }
                        }
                    }
                }

                ImportedFileList = Utils.CreateFileList(importedFiles);
                ImportResult = importedFiles.Count > 0 ? $"{importedFiles.Count} files imported." : string.Empty;

                model.NotifyFilesImported(importedFiles);
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }

            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));
        }

        private void CopyImportedFileList()
        {
            ClipboardService.SetText(ImportedFileList);
        }

        private void RemoveFileListMethod()
        {
            var fileIds = Utils.CreateFileIds(RemoveFileList);
            if (fileIds.Count == 0)
            {
                Dialogs.ShowErrorDialog("No file ids specified");
                return;
            }

            if (Dialogs.ShowConfirmDialog($"Remove meta-data for {fileIds.Count} files from the specified file list?"))
            {
                fileIds.ForEach(x => model.DbAccess.DeleteFile(x));
            }
        }

        private string GetDateModified(string internalPath)
        {
            var path = model.FilesystemAccess.ToAbsolutePath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
