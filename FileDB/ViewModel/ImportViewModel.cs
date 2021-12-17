using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDBInterface;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using Newtonsoft.Json;
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
        public ICommand ScanNewFilesCommand => scanNewFilesCommand ??= new CommandHandler(ScanNewFiles);
        private ICommand scanNewFilesCommand;

        public ICommand ImportNewFilesCommand => importNewFilesCommand ??= new CommandHandler(ImportNewFiles);
        private ICommand importNewFilesCommand;

        public ICommand CopyImportedFileListCommand => copyImportedFileListCommand ??= new CommandHandler(CopyImportedFileList);
        private ICommand copyImportedFileListCommand;

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

        private readonly Model.Model model = Model.Model.Instance;

        public ImportViewModel()
        {
        }

        public void ScanNewFiles()
        {
            if (!Utils.ShowConfirmDialog($"Find all files, not yet imported, from directory {model.Config.FilesRootDirectory}?"))
            {
                return;
            }

            NewFiles.Clear();
            ImportResult = string.Empty;
            ImportedFileList = string.Empty;
            OnPropertyChanged(nameof(NewFilesAvailable));

            var blacklistedFilePathPatterns = model.Config.BlacklistedFilePathPatterns.Split(";");
            var whitelistedFilePathPatterns = model.Config.WhitelistedFilePathPatterns.Split(";");

            foreach (var internalFilePath in model.FilesystemAccess.ListNewFilesystemFiles(blacklistedFilePathPatterns, whitelistedFilePathPatterns, model.Config.IncludeHiddenDirectories, model.DbAccess))
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
                Utils.ShowInfoDialog($"No new files found. Add files to directory {model.Config.FilesRootDirectory} or configure another files root directory.");
            }
        }

        public void ImportNewFiles()
        {
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
                Utils.ShowErrorDialog(e.Message);
            }

            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));
        }

        private void CopyImportedFileList()
        {
            ClipboardService.SetText(ImportedFileList);
        }

        private string GetDateModified(string internalPath)
        {
            var path = model.FilesystemAccess.ToAbsolutePath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
