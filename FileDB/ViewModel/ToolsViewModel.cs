using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Export;
using FileDB.Model;
using FileDBShared.Model;
using FileDBShared.Validators;
using TextCopy;

namespace FileDB.ViewModel;

public partial class ToolsViewModel : ObservableObject
{
    private const string ToolNotExecutedText = "Not executed.";

    [ObservableProperty]
    private string backupResult = ToolNotExecutedText;

    [ObservableProperty]
    private string backupListHeader = string.Empty;

    [ObservableProperty]
    private string cacheResult = ToolNotExecutedText;

    public ObservableCollection<BackupFile> BackupFiles { get; } = new();

    [ObservableProperty]
    private string findImportedNoLongerApplicableFilesResult = ToolNotExecutedText;

    [ObservableProperty]
    private string importedNoLongerApplicableFileList = string.Empty;

    [ObservableProperty]
    private string databaseValidationResult = ToolNotExecutedText;

    public ObservableCollection<string> DabaseValidationErrors { get; } = new();

    [ObservableProperty]
    private string invalidFileList = string.Empty;

    [ObservableProperty]
    private string fileFinderResult = ToolNotExecutedText;

    [ObservableProperty]
    private string missingFilesList = string.Empty;

    [ObservableProperty]
    private string databaseExportDirectory = string.Empty;

    [ObservableProperty]
    private string databaseExportResult = ToolNotExecutedText;

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;

    public ToolsViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IDialogs dialogs, IFileSystem fileSystem)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;
        ScanBackupFiles();
    }

    [RelayCommand]
    private void CreateBackup()
    {
        try
        {
            new DatabaseBackup(configRepository, fileSystem).CreateBackup();
            BackupResult = "Backup created.";
            ScanBackupFiles();
        }
        catch (IOException e)
        {
            BackupResult = e.Message;
            dialogs.ShowErrorDialog(e.Message);
        }
    }

    [RelayCommand]
    private void OpenDatabaseBackupDirectory()
    {
        Utils.OpenDirectoryInExplorer(new DatabaseBackup(configRepository, fileSystem).BackupDirectory);
    }

    private void ScanBackupFiles()
    {
        BackupFiles.Clear();

        var backupHandler = new DatabaseBackup(configRepository, fileSystem);

        if (Directory.Exists(backupHandler.BackupDirectory))
        {
            foreach (var backupFile in backupHandler.ListAvailableBackupFiles())
            {
                BackupFiles.Add(backupFile);
            }

            BackupListHeader = BackupFiles.Count > 0 ? $"Database backup files:" : $"No database backup files found!";
        }
        else
        {
            BackupListHeader = "Directory for configured database does not exist.";
        }
    }

    [RelayCommand]
    private void FindImportedNoLongerApplicableFiles()
    {
        var blacklistedFilePathPatterns = configRepository.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configRepository.Config.WhitelistedFilePathPatterns.Split(";");
        var notApplicableFiles = dbAccessRepository.DbAccess.GetFiles().Where(x => !filesystemAccessRepository.FilesystemAccess.PathIsApplicable(x.Path, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configRepository.Config.IncludeHiddenDirectories)).ToList();
        ImportedNoLongerApplicableFileList = Utils.CreateFileList(notApplicableFiles);
        FindImportedNoLongerApplicableFilesResult = $"Found {notApplicableFiles.Count} files that now should be filtered.";
    }

    [RelayCommand]
    private void CopyImportedNoLongerApplicableFilesList()
    {
        ClipboardService.SetText(ImportedNoLongerApplicableFileList);
    }

    [RelayCommand]
    private void DatabaseValidation()
    {
        DabaseValidationErrors.Clear();

        var filesValidator = new FilesModelValidator();
        List<FilesModel> invalidFiles = new();
        foreach (var file in dbAccessRepository.DbAccess.GetFiles())
        {
            var result = filesValidator.Validate(file);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add($"File {file.Id}: {error.ErrorMessage}");
                }
                invalidFiles.Add(file);
            }
        }
        InvalidFileList = Utils.CreateFileList(invalidFiles);

        var personValidator = new PersonModelValidator();
        foreach (var person in dbAccessRepository.DbAccess.GetPersons())
        {
            var result = personValidator.Validate(person);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add($"Person {person.Id}: {error.ErrorMessage}");
                }
            }
        }

        var locationValidator = new LocationModelValidator();
        foreach (var location in dbAccessRepository.DbAccess.GetLocations())
        {
            var result = locationValidator.Validate(location);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add($"Location {location.Id}: {error.ErrorMessage}");
                }
            }
        }

        var tagValidator = new TagModelValidator();
        foreach (var tag in dbAccessRepository.DbAccess.GetTags())
        {
            var result = tagValidator.Validate(tag);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add($"Tag {tag.Id}: {error.ErrorMessage}");
                }
            }
        }

        DatabaseValidationResult = DabaseValidationErrors.Count > 0 ? $"{DabaseValidationErrors.Count} errors found:" : $"No errors found.";
        OnPropertyChanged(nameof(DabaseValidationErrors));
    }

    [RelayCommand]
    private void CopyInvalidFileList()
    {
        ClipboardService.SetText(InvalidFileList);
        DatabaseValidationResult = "File list copied to clipboard.";
    }

    [RelayCommand]
    private void FileFinder()
    {
        FileFinderResult = "Running, please wait...";

        List<FilesModel> missingFiles = new();
        foreach (var file in filesystemAccessRepository.FilesystemAccess.GetFilesMissingInFilesystem(dbAccessRepository.DbAccess.GetFiles()))
        {
            missingFiles.Add(file);
        }

        FileFinderResult = missingFiles.Count == 0 ? "No missing files found." : $"{missingFiles.Count} meta-data for missing files found.";
        MissingFilesList = Utils.CreateFileList(missingFiles);
    }

    [RelayCommand]
    private void CopyFileFinderResult()
    {
        ClipboardService.SetText(MissingFilesList);
        FileFinderResult = "File list copied to clipboard.";
    }

    [RelayCommand]
    private void DatabaseExport()
    {
        if (!Directory.Exists(DatabaseExportDirectory))
        {
            DatabaseExportResult = "No such directory.";
            return;
        }

        if (Directory.GetFileSystemEntries(DatabaseExportDirectory).Length > 0)
        {
            DatabaseExportResult = "Specified directory is not empty.";
            return;
        }

        try
        {
            var exporter = new DatabaseExporter(DatabaseExportDirectory);
            var persons = dbAccessRepository.DbAccess.GetPersons().ToList();
            var locations = dbAccessRepository.DbAccess.GetLocations().ToList();
            var tags = dbAccessRepository.DbAccess.GetTags().ToList();
            var files = dbAccessRepository.DbAccess.GetFiles().ToList();
            exporter.Export(persons, locations, tags, files);
            DatabaseExportResult = $"Exported {persons.Count} persons, {locations.Count} locations, {tags.Count} tags and {files.Count} files.";
        }
        catch (Exception e)
        {
            DatabaseExportResult = e.Message;
        }
    }
}
