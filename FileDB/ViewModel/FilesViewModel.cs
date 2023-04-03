using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
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

    private IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IDialogs dialogs;

    public FilesViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IDialogs dialogs)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.dialogs = dialogs;

        subdirToScan = configRepository.Config.FilesRootDirectory;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            SubdirToScan = configRepository.Config.FilesRootDirectory;
        });
    }

    [RelayCommand]
    private void BrowseSubDirectory()
    {
        SubdirToScan = dialogs.BrowseExistingDirectory(configRepository.Config.FilesRootDirectory, "Select a sub directory") ?? string.Empty;
    }

    [RelayCommand]
    private void ScanNewFiles()
    {
        ScanNewFiles(configRepository.Config.FilesRootDirectory);
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
        if (!SubdirToScan.StartsWith(configRepository.Config.FilesRootDirectory))
        {
            dialogs.ShowErrorDialog($"Specified directory is not within the configured files root directory: {configRepository.Config.FilesRootDirectory}");
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

        var blacklistedFilePathPatterns = configRepository.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configRepository.Config.WhitelistedFilePathPatterns.Split(";");

        dialogs.ShowProgressDialog(progress =>
        {
            progress.Report("Scanning...");

            foreach (var internalFilePath in filesystemAccessRepository.FilesystemAccess.ListNewFilesystemFiles(pathToScan, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configRepository.Config.IncludeHiddenDirectories, dbAccessRepository.DbAccess))
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
                    dialogs.ShowInfoDialog($"No new files found. Add your files to '{configRepository.Config.FilesRootDirectory}'.");
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
            var locations = dbAccessRepository.DbAccess.GetLocations();

            try
            {
                List<FileModel> importedFiles = new();

                var counter = 1;
                foreach (var fileToAdd in filesToAdd)
                {
                    progress.Report($"Adding file {counter} / {filesToAdd.Count}...");
                    Thread.Sleep(1000);

                    dbAccessRepository.DbAccess.InsertFile(fileToAdd.Path, null, filesystemAccessRepository.FilesystemAccess, FindFileMetadata);

                    var importedFile = dbAccessRepository.DbAccess.GetFileByPath(fileToAdd.Path);

                    if (importedFile != null)
                    {
                        importedFiles.Add(importedFile);

                        if (importedFile.Position != null && configRepository.Config.FileToLocationMaxDistance > 0.5)
                        {
                            var importedFilePos = DatabaseParsing.ParseFilesPosition(importedFile.Position)!.Value;

                            foreach (var locationWithPosition in locations.Where(x => x.Position != null))
                            {
                                var locationPos = DatabaseParsing.ParseFilesPosition(locationWithPosition.Position)!.Value;
                                var distance = DatabaseUtils.CalculateDistance(importedFilePos.lat, importedFilePos.lon, locationPos.lat, locationPos.lon);
                                if (distance < configRepository.Config.FileToLocationMaxDistance)
                                {
                                    dbAccessRepository.DbAccess.InsertFileLocation(importedFile.Id, locationWithPosition.Id);
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
            fileIds.ForEach(x => dbAccessRepository.DbAccess.DeleteFile(x));
            dialogs.ShowInfoDialog($"{fileIds.Count} files removed");
        }
    }

    private string GetDateModified(string internalPath)
    {
        var path = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(internalPath);
        var dateModified = File.GetLastWriteTime(path);
        return dateModified.ToString("yyyy-MM-dd HH:mm");
    }

    public void SelectionChanged()
    {
        OnPropertyChanged(nameof(NewFilesSelected));
    }
}
