using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels;

public record NewFile(string Path, string DateModified);

public partial class FilesViewModel : ObservableObject
{
    [ObservableProperty]
    private string subdirToScan;

    public ObservableCollection<NewFile> NewFiles { get; } = [];

    public ObservableCollection<NewFile> SelectedFiles { get; } = [];

    [ObservableProperty]
    private string importResult = string.Empty;

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private string removeFileList = string.Empty;

    [ObservableProperty]
    private bool findFileMetadata = true;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;
    private readonly IClipboardService clipboardService;
    private readonly ICriteriaViewModel criteriaViewModel;

    public FilesViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IDialogs dialogs, IFileSystem fileSystem, IClipboardService clipboardService, ICriteriaViewModel criteriaViewModel)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;
        this.clipboardService = clipboardService;
        this.criteriaViewModel = criteriaViewModel;

        subdirToScan = configProvider.FilePaths.FilesRootDir;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            SubdirToScan = configProvider.FilePaths.FilesRootDir;
        });
    }

    [RelayCommand]
    private async Task BrowseSubDirectoryAsync()
    {
        var selectedDir = await dialogs.ShowBrowseExistingSubDirectoryDialogAsync(Strings.FilesSelectASubDirectory, configProvider.FilePaths.FilesRootDir);
        if (selectedDir is not null)
        {
            SubdirToScan = Path.Combine(configProvider.FilePaths.FilesRootDir, selectedDir);
        }
    }

    [RelayCommand]
    private async Task ScanNewFilesAsync()
    {
        await ScanNewFilesAsync(configProvider.FilePaths.FilesRootDir);
    }

    [RelayCommand]
    private async Task ScanNewFilesInDirectoryAsync()
    {
        if (!SubdirToScan.HasContent())
        {
            await dialogs.ShowErrorDialogAsync(Strings.FilesNoDirectorySpecified);
            return;
        }
        if (!filesystemAccessProvider.FilesystemAccess.FileSystem.Directory.Exists(SubdirToScan))
        {
            await dialogs.ShowErrorDialogAsync(Strings.FilesSpecifiedDirectoryDoesNotExist);
            return;
        }
        if (!SubdirToScan.StartsWith(configProvider.FilePaths.FilesRootDir))
        {
            await dialogs.ShowErrorDialogAsync(string.Format(Strings.FilesSpecifiedDirectoryIsNotWithinTheConfiguredFilesRootDirectory, configProvider.FilePaths.FilesRootDir));
            return;
        }
        await ScanNewFilesAsync(SubdirToScan);
    }

    public async Task ScanNewFilesAsync(string pathToScan)
    {
        if (!await dialogs.ShowConfirmDialogAsync(string.Format(Strings.FilesFindAllFiles, pathToScan)))
        {
            return;
        }

        NewFiles.Clear();
        ImportResult = string.Empty;
        ImportedFileList = string.Empty;

        var blacklistedFilePathPatterns = configProvider.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configProvider.Config.WhitelistedFilePathPatterns.Split(";");

        dialogs.ShowProgressDialog(progress =>
        {
            progress.Report(Strings.FilesScanning);

            foreach (var internalFilePath in filesystemAccessProvider.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configProvider.Config.IncludeHiddenDirectories, dbAccessProvider.DbAccess))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    NewFiles.Add(new NewFile(internalFilePath, GetDateModified(internalFilePath)));
                });
                progress.Report(string.Format(Strings.FilesScanningNewFiles, NewFiles.Count));
            }

            Dispatcher.UIThread.Invoke(async () =>
            {
                OnPropertyChanged(nameof(NewFiles));

                if (NewFiles.Count == 0)
                {
                    await dialogs.ShowInfoDialogAsync(string.Format(Strings.FilesNoNewFilesFound, configProvider.FilePaths.FilesRootDir));
                }
                else
                {
                    SelectAll();
                }
            });
        });
    }

    [RelayCommand]
    private void SelectAll()
    {
        SelectedFiles.Clear();
        foreach (var file in NewFiles)
        {
            SelectedFiles.Add(file);
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        SelectedFiles.Clear();
    }

    [RelayCommand]
    private async Task ImportNewFilesAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync(string.Format(Strings.FilesAddMetaDataFromFiles, SelectedFiles.Count)))
        {
            return;
        }

        try
        {
            new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).CreateBackup();
        }
        catch (Exception e)
        {
            await dialogs.ShowErrorDialogAsync(Strings.FilesUnableToCreateDatabaseBackup, e);
            return;
        }

        dialogs.ShowProgressDialog(progress =>
        {
            var locations = dbAccessProvider.DbAccess.GetLocations();

            try
            {
                List<FileModel> addedFiles = [];

                var counter = 1;
                foreach (var fileToAdd in SelectedFiles)
                {
                    progress.Report(string.Format(Strings.FilesAddingFile, counter, SelectedFiles.Count));
                    Thread.Sleep(1000);

                    dbAccessProvider.DbAccess.InsertFile(fileToAdd.Path, null, filesystemAccessProvider.FilesystemAccess, FindFileMetadata);

                    var importedFile = dbAccessProvider.DbAccess.GetFileByPath(fileToAdd.Path);

                    if (importedFile is not null)
                    {
                        addedFiles.Add(importedFile);

                        if (importedFile.Position is not null && configProvider.Config.FileToLocationMaxDistance > 0.5)
                        {
                            var (lat, lon) = DatabaseParsing.ParseFilesPosition(importedFile.Position)!.Value;

                            foreach (var locationWithPosition in locations.Where(x => x.Position is not null))
                            {
                                var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position)!.Value;
                                var distance = LatLonUtils.CalculateDistance(lat, lon, locationPos.lat, locationPos.lon);
                                if (distance < configProvider.Config.FileToLocationMaxDistance)
                                {
                                    dbAccessProvider.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
                                }
                            }
                        }
                    }

                    counter++;
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    ImportedFileList = Utils.CreateFileList(addedFiles);
                    ImportResult = addedFiles.Count > 0 ? string.Format(Strings.FilesFilesAdded, addedFiles.Count) : string.Empty;

                    foreach (var selectedFile in SelectedFiles.ToList())
                    {
                        NewFiles.Remove(selectedFile);
                    }
                });
            }
            catch (DataValidationException e)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    dialogs.ShowErrorDialogAsync(e.Message);
                    foreach (var selectedFile in SelectedFiles.ToList())
                    {
                        NewFiles.Remove(selectedFile);
                    }
                });
            }
        });
    }

    [RelayCommand]
    private void SearchImportedFileList() => criteriaViewModel.SearchForFilesAsync(ImportedFileList);

    [RelayCommand]
    private async Task RemoveFileListMethodAsync()
    {
        var fileIds = Utils.CreateFileIds(RemoveFileList);
        if (fileIds.Count == 0)
        {
            await dialogs.ShowErrorDialogAsync(Strings.FilesNoFileIdsSpecified);
            return;
        }

        if (await dialogs.ShowConfirmDialogAsync(string.Format(Strings.FilesRemoveMetaDataFor, fileIds.Count)))
        {
            fileIds.ForEach(x => dbAccessProvider.DbAccess.DeleteFile(x));
            await dialogs.ShowInfoDialogAsync(string.Format(Strings.FilesFilesRemoved, fileIds.Count));
        }
    }

    private string GetDateModified(string internalPath)
    {
        var path = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(internalPath);
        var dateModified = fileSystem.File.GetLastWriteTime(path);
        return dateModified.ToDateAndTime();
    }
}
