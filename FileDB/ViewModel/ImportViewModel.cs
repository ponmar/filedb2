using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FileDBInterface;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using Newtonsoft.Json;

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
            if (!Utils.ShowConfirmDialog($"Find all files, not yet imported, from directory {Utils.Config.FilesRootDirectory}?"))
            {
                return;
            }

            NewFiles.Clear();
            OnPropertyChanged(nameof(NewFilesAvailable));

            var blacklistedFilePathPatterns = Utils.Config.BlacklistedFilePathPatterns.Split(";");
            var whitelistedFilePathPatterns = Utils.Config.WhitelistedFilePathPatterns.Split(";");

            foreach (var internalFilePath in Utils.FilesystemAccess.ListNewFilesystemFiles(blacklistedFilePathPatterns, whitelistedFilePathPatterns, Utils.Config.IncludeHiddenDirectories, Utils.DbAccess))
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
                Utils.ShowWarningDialog($"No new files found. Add files to directory {Utils.Config.FilesRootDirectory} or configure another files root directory.");
            }
        }

        public void ImportNewFiles()
        {
            var locations = Utils.DbAccess.GetLocations();

            try
            {
                foreach (var newFile in NewFiles)
                {
                    Utils.DbAccess.InsertFile(newFile.Path, null, Utils.FilesystemAccess);

                    var importedFile = Utils.DbAccess.GetFileByPath(newFile.Path);

                    if (importedFile != null && importedFile.Position != null && Utils.Config.FileToLocationMaxDistance > 0.5)
                    {
                        var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position).Value;

                        foreach (var locationWithPosition in locations.Where(x => x.Position != null))
                        {
                            var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position).Value;
                            var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                            if (distance < Utils.Config.FileToLocationMaxDistance)
                            {
                                Utils.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
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
            var path = Utils.FilesystemAccess.ToAbsolutePath(internalPath);
            var dateModified = File.GetLastWriteTime(path);
            return dateModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
