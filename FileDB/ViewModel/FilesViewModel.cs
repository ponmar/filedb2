using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;
using TextCopy;

namespace FileDB.ViewModel;

public class NewFile
{
    public string Path { get; }

    public string DateModified { get; }

    public bool IsSelected { get; set; } = false;

    public NewFile(string path, string dateModified)
    {
        Path = path;
        DateModified = dateModified;
    }
}

public partial class FilesViewModel : ObservableObject
{
    [ObservableProperty]
    private string subdirToScan;

    public ObservableCollection<NewFile> NewFiles { get; } = new();

    public bool NewFilesSelected => NewFiles.Any(x => x.IsSelected);

    [ObservableProperty]
    private string importResult = string.Empty;

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private string removeFileList = string.Empty;

    [ObservableProperty]
    private bool findFileMetadata = true;

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly IFilesystemAccess filesystemAccess;
    private readonly IDialogs dialogs;

    public FilesViewModel(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess, IDialogs dialogs)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        this.filesystemAccess = filesystemAccess;
        this.dialogs = dialogs;

        subdirToScan = config.FilesRootDirectory;

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            this.config = x.Config;
            SubdirToScan = this.config.FilesRootDirectory;
        });
    }

    [RelayCommand]
    private void BrowseSubDirectory()
    {
        SubdirToScan = dialogs.BrowseExistingDirectory(config.FilesRootDirectory, "Select a sub directory") ?? string.Empty;
    }

    [RelayCommand]
    private void ScanNewFiles()
    {
        ScanNewFiles(config.FilesRootDirectory);
    }

    [RelayCommand]
    private void ScanNewFilesInDirectory()
    {
        if (string.IsNullOrEmpty(SubdirToScan))
        {
            dialogs.ShowErrorDialog("No directory specified");
            return;
        }
        if (!Directory.Exists(SubdirToScan))
        {
            dialogs.ShowErrorDialog("Specified directory does no exist");
            return;
        }
        if (!SubdirToScan.StartsWith(config.FilesRootDirectory))
        {
            dialogs.ShowErrorDialog($"Specified directory is not within the configured files root directory: {config.FilesRootDirectory}");
            return;
        }
        ScanNewFiles(SubdirToScan);
    }

    public void ScanNewFiles(string pathToScan)
    {
        if (!dialogs.ShowConfirmDialog($"Find all files, not yet imported, from '{pathToScan}'?"))
        {
            return;
        }

        NewFiles.Clear();
        ImportResult = string.Empty;
        ImportedFileList = string.Empty;
        OnPropertyChanged(nameof(NewFilesSelected));

        var blacklistedFilePathPatterns = config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = config.WhitelistedFilePathPatterns.Split(";");

        dialogs.ShowProgressDialog(progress =>
        {
            progress.Report("Scanning...");

            foreach (var internalFilePath in filesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, config.IncludeHiddenDirectories, dbAccess))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NewFiles.Add(new NewFile(internalFilePath, GetDateModified(internalFilePath)));
                }));
                progress.Report($"Scanning... New files: {NewFiles.Count}");
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OnPropertyChanged(nameof(NewFilesSelected));
                OnPropertyChanged(nameof(NewFiles));

                if (NewFiles.Count == 0)
                {
                    dialogs.ShowInfoDialog($"No new files found. Add your files to '{config.FilesRootDirectory}'.");
                }
            }));
        });
    }

    [RelayCommand]
    private void ImportNewFiles()
    {
        var filesToAdd = NewFiles.Where(x => x.IsSelected).ToList();
        if (!dialogs.ShowConfirmDialog($"Add meta-data from {filesToAdd.Count} files?"))
        {
            return;
        }

        dialogs.ShowProgressDialog(progress =>
        {
            var locations = dbAccess.GetLocations();

            try
            {
                List<FilesModel> importedFiles = new();

                var counter = 1;
                foreach (var fileToAdd in filesToAdd)
                {
                    progress.Report($"Adding file {counter} / {filesToAdd.Count}...");
                    Thread.Sleep(1000);

                    dbAccess.InsertFile(fileToAdd.Path, null, filesystemAccess, FindFileMetadata);

                    var importedFile = dbAccess.GetFileByPath(fileToAdd.Path);

                    if (importedFile != null)
                    {
                        importedFiles.Add(importedFile);

                        if (importedFile.Position != null && config.FileToLocationMaxDistance > 0.5)
                        {
                            var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position)!.Value;

                            foreach (var locationWithPosition in locations.Where(x => x.Position != null))
                            {
                                var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position)!.Value;
                                var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                                if (distance < config.FileToLocationMaxDistance)
                                {
                                    dbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
                                }
                            }
                        }
                    }

                    counter++;
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImportedFileList = Utils.CreateFileList(importedFiles);
                    ImportResult = importedFiles.Count > 0 ? $"{importedFiles.Count} files added." : string.Empty;

                    Events.Send(new FilesImported(importedFiles));

                    filesToAdd.ForEach(x => NewFiles.Remove(x));
                    OnPropertyChanged(nameof(NewFilesSelected));
                }));
            }
            catch (DataValidationException e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dialogs.ShowErrorDialog(e.Message);

                    filesToAdd.ForEach(x => NewFiles.Remove(x));
                    OnPropertyChanged(nameof(NewFilesSelected));
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
            dialogs.ShowErrorDialog("No file ids specified");
            return;
        }

        if (dialogs.ShowConfirmDialog($"Remove meta-data for {fileIds.Count} files from the specified file list?"))
        {
            fileIds.ForEach(x => dbAccess.DeleteFile(x));
            dialogs.ShowInfoDialog($"{fileIds.Count} files removed");
        }
    }

    private string GetDateModified(string internalPath)
    {
        var path = filesystemAccess.ToAbsolutePath(internalPath);
        var dateModified = File.GetLastWriteTime(path);
        return dateModified.ToString("yyyy-MM-dd HH:mm");
    }

    public void SelectionChanged()
    {
        OnPropertyChanged(nameof(NewFilesSelected));
    }
}
