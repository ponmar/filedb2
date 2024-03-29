using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBShared;
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

    public ObservableCollection<NewFile> NewFiles { get; } = [];

    public bool NewFilesSelected => NewFiles.Any(x => x.IsSelected);

    [ObservableProperty]
    private string importResult = string.Empty;

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private string removeFileList = string.Empty;

    [ObservableProperty]
    private bool findFileMetadata = true;

    private IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;

    public FilesViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IDialogs dialogs, IFileSystem fileSystem)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;

        subdirToScan = configProvider.FilePaths.FilesRootDir;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            SubdirToScan = configProvider.FilePaths.FilesRootDir;
        });
    }

    [RelayCommand]
    private void BrowseSubDirectory()
    {
        SubdirToScan = dialogs.BrowseExistingDirectory(configProvider.FilePaths.FilesRootDir, "Select a sub directory") ?? string.Empty;
    }

    [RelayCommand]
    private void ScanNewFiles()
    {
        ScanNewFiles(configProvider.FilePaths.FilesRootDir);
    }

    [RelayCommand]
    private void ScanNewFilesInDirectory()
    {
        if (!SubdirToScan.HasContent())
        {
            dialogs.ShowErrorDialog("No directory specified");
            return;
        }
        if (!Directory.Exists(SubdirToScan))
        {
            dialogs.ShowErrorDialog("Specified directory does no exist");
            return;
        }
        if (!SubdirToScan.StartsWith(configProvider.FilePaths.FilesRootDir))
        {
            dialogs.ShowErrorDialog($"Specified directory is not within the configured files root directory: {configProvider.FilePaths.FilesRootDir}");
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

        var blacklistedFilePathPatterns = configProvider.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configProvider.Config.WhitelistedFilePathPatterns.Split(";");

        dialogs.ShowProgressDialog(progress =>
        {
            progress.Report("Scanning...");

            foreach (var internalFilePath in filesystemAccessProvider.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configProvider.Config.IncludeHiddenDirectories, dbAccessProvider.DbAccess))
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
                    dialogs.ShowInfoDialog($"No new files found. Add your files to '{configProvider.FilePaths.FilesRootDir}'.");
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

        try
        {
            new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).CreateBackup();
        }
        catch (Exception e)
        {
            dialogs.ShowErrorDialog("Unable to create database backup", e);
            return;
        }

        dialogs.ShowProgressDialog(progress =>
        {
            var locations = dbAccessProvider.DbAccess.GetLocations();

            try
            {
                List<FileModel> importedFiles = [];

                var counter = 1;
                foreach (var fileToAdd in filesToAdd)
                {
                    progress.Report($"Adding file {counter} / {filesToAdd.Count}...");
                    Thread.Sleep(1000);

                    dbAccessProvider.DbAccess.InsertFile(fileToAdd.Path, null, filesystemAccessProvider.FilesystemAccess, FindFileMetadata);

                    var importedFile = dbAccessProvider.DbAccess.GetFileByPath(fileToAdd.Path);

                    if (importedFile is not null)
                    {
                        importedFiles.Add(importedFile);

                        if (importedFile.Position is not null && configProvider.Config.FileToLocationMaxDistance > 0.5)
                        {
                            var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position)!.Value;

                            foreach (var locationWithPosition in locations.Where(x => x.Position is not null))
                            {
                                var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position)!.Value;
                                var distance = LatLonUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                                if (distance < configProvider.Config.FileToLocationMaxDistance)
                                {
                                    dbAccessProvider.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
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
            fileIds.ForEach(x => dbAccessProvider.DbAccess.DeleteFile(x));
            dialogs.ShowInfoDialog($"{fileIds.Count} files removed");
        }
    }

    private string GetDateModified(string internalPath)
    {
        var path = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(internalPath);
        var dateModified = fileSystem.File.GetLastWriteTime(path);
        return dateModified.ToDateAndTime();
    }

    public void SelectionChanged()
    {
        OnPropertyChanged(nameof(NewFilesSelected));
    }
}
