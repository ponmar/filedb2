using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using TextCopy;

namespace FileDB.ViewModel
{
    public class NewFile
    {
        public string Path { get; }

        public string DateModified { get; }

        public NewFile(string path, string dateModified)
        {
            Path = path;
            DateModified = dateModified;
        }
    }

    public partial class ImportViewModel : ObservableObject
    {
        [ObservableProperty]
        private string subdirToScan;

        public ObservableCollection<NewFile> NewFiles { get; } = new();

        public bool NewFilesAvailable => NewFiles.Count > 0;

        [ObservableProperty]
        private string importResult = string.Empty;

        [ObservableProperty]
        private string importedFileList = string.Empty;

        [ObservableProperty]
        private string removeFileList = string.Empty;

        private readonly Model.Model model = Model.Model.Instance;

        public ImportViewModel()
        {
            subdirToScan = model.Config.FilesRootDirectory;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object? sender, EventArgs e)
        {
            SubdirToScan = model.Config.FilesRootDirectory;
        }

        [RelayCommand]
        private void ScanNewFiles()
        {
            ScanNewFiles(model.Config.FilesRootDirectory);
        }

        [RelayCommand]
        private void ScanNewFilesInDirectory()
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
            ScanNewFiles(SubdirToScan);
        }

        public void ScanNewFiles(string pathToScan)
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

            Dialogs.ShowProgressDialog(progress =>
            {
                progress.Report("Scanning...");

                foreach (var internalFilePath in model.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, model.Config.IncludeHiddenDirectories, model.DbAccess))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NewFiles.Add(new NewFile(internalFilePath, GetDateModified(internalFilePath)));
                    }));
                    progress.Report($"Scanning... New files: {NewFiles.Count}");
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    OnPropertyChanged(nameof(NewFilesAvailable));
                    OnPropertyChanged(nameof(NewFiles));

                    if (NewFiles.Count == 0)
                    {
                        Dialogs.ShowInfoDialog($"No new files found. Add your files to '{model.Config.FilesRootDirectory}'.");
                    }
                }));
            });
        }

        [RelayCommand]
        private void ImportNewFiles()
        {
            if (!Dialogs.ShowConfirmDialog($"Import meta-data from {NewFiles.Count} files?"))
            {
                return;
            }

            Dialogs.ShowProgressDialog(progress =>
            {
                var locations = model.DbAccess.GetLocations();

                try
                {
                    List<FilesModel> importedFiles = new();

                    var counter = 1;
                    foreach (var newFile in NewFiles)
                    {
                        progress.Report($"Adding file {counter} / {NewFiles.Count}...");
                        Thread.Sleep(1000);

                        model.DbAccess.InsertFile(newFile.Path, null, model.FilesystemAccess);

                        var importedFile = model.DbAccess.GetFileByPath(newFile.Path);

                        if (importedFile != null)
                        {
                            importedFiles.Add(importedFile);

                            if (importedFile.Position != null && model.Config.FileToLocationMaxDistance > 0.5)
                            {
                                var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position)!.Value;

                                foreach (var locationWithPosition in locations.Where(x => x.Position != null))
                                {
                                    var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position)!.Value;
                                    var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                                    if (distance < model.Config.FileToLocationMaxDistance)
                                    {
                                        model.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
                                    }
                                }
                            }
                        }

                        counter++;
                    }

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ImportedFileList = Utils.CreateFileList(importedFiles);
                        ImportResult = importedFiles.Count > 0 ? $"{importedFiles.Count} files imported." : string.Empty;

                        model.NotifyFilesImported(importedFiles);

                        NewFiles.Clear();
                        OnPropertyChanged(nameof(NewFilesAvailable));
                    }));
                }
                catch (DataValidationException e)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Dialogs.ShowErrorDialog(e.Message);

                        NewFiles.Clear();
                        OnPropertyChanged(nameof(NewFilesAvailable));
                    }));
                }
            });
        }

        [RelayCommand]
        private void CopyImportedFileList()
        {
            ClipboardService.SetText(ImportedFileList);
        }

        [RelayCommand]
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
