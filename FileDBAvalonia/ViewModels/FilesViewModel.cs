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
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBShared;
using FileDBShared.Model;
using TextCopy;

namespace FileDBAvalonia.ViewModels;

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
    private async Task BrowseSubDirectoryAsync()
    {
        var selectedDir = await dialogs.ShowBrowseExistingDirectoryDialogAsync("Select a sub directory", configProvider.FilePaths.FilesRootDir);
        SubdirToScan = selectedDir ?? string.Empty;
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
            await dialogs.ShowErrorDialogAsync("No directory specified");
            return;
        }
        if (!filesystemAccessProvider.FilesystemAccess.FileSystem.Directory.Exists(SubdirToScan))
        {
            await dialogs.ShowErrorDialogAsync("Specified directory does no exist");
            return;
        }
        if (!SubdirToScan.StartsWith(configProvider.FilePaths.FilesRootDir))
        {
            await dialogs.ShowErrorDialogAsync($"Specified directory is not within the configured files root directory: {configProvider.FilePaths.FilesRootDir}");
            return;
        }
        await ScanNewFilesAsync(SubdirToScan);
    }

    public async Task ScanNewFilesAsync(string pathToScan)
    {
        if (!await dialogs.ShowConfirmDialogAsync($"Find all files, not yet imported, from '{pathToScan}'?\n\n{Strings.FilesWarningText}"))
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
            progress.Report("Scanning...");

            foreach (var internalFilePath in filesystemAccessProvider.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configProvider.Config.IncludeHiddenDirectories, dbAccessProvider.DbAccess))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    NewFiles.Add(new NewFile(internalFilePath, GetDateModified(internalFilePath)));
                });
                progress.Report($"Scanning... New files: {NewFiles.Count}");
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                OnPropertyChanged(nameof(NewFiles));

                if (NewFiles.Count == 0)
                {
                    dialogs.ShowInfoDialogAsync($"No new files found. Add your files to '{configProvider.FilePaths.FilesRootDir}'.");
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
        if (!await dialogs.ShowConfirmDialogAsync($"Add meta-data from {SelectedFiles.Count} files?"))
        {
            return;
        }

        try
        {
            new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).CreateBackup();
        }
        catch (Exception e)
        {
            await dialogs.ShowErrorDialogAsync("Unable to create database backup", e);
            return;
        }

        dialogs.ShowProgressDialog(progress =>
        {
            var locations = dbAccessProvider.DbAccess.GetLocations();

            try
            {
                List<FileModel> importedFiles = [];

                var counter = 1;
                foreach (var fileToAdd in SelectedFiles)
                {
                    progress.Report($"Adding file {counter} / {SelectedFiles.Count}...");
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

                Dispatcher.UIThread.Invoke(() =>
                {
                    ImportedFileList = Utils.CreateFileList(importedFiles);
                    ImportResult = importedFiles.Count > 0 ? $"{importedFiles.Count} files added." : string.Empty;

                    Messenger.Send(new FilesImported(importedFiles));

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
    private void CopyImportedFileList()
    {
        ClipboardService.SetText(ImportedFileList);
    }

    [RelayCommand]
    private async Task RemoveFileListMethodAsync()
    {
        var fileIds = Utils.CreateFileIds(RemoveFileList);
        if (fileIds.Count == 0)
        {
            await dialogs.ShowErrorDialogAsync("No file ids specified");
            return;
        }

        if (await dialogs.ShowConfirmDialogAsync($"Remove meta-data for {fileIds.Count} files from the specified file list?"))
        {
            fileIds.ForEach(x => dbAccessProvider.DbAccess.DeleteFile(x));
            await dialogs.ShowInfoDialogAsync($"{fileIds.Count} files removed");
        }
    }

    private string GetDateModified(string internalPath)
    {
        var path = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(internalPath);
        var dateModified = fileSystem.File.GetLastWriteTime(path);
        return dateModified.ToDateAndTime();
    }
}
