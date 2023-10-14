using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Export;
using FileDB.Model;
using FileDB.Resources;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBShared.Model;
using FileDBShared.Validators;
using TextCopy;

namespace FileDB.ViewModel;

public partial class ToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private string backupListHeader = string.Empty;

    public ObservableCollection<BackupFile> BackupFiles { get; } = new();

    [ObservableProperty]
    private string importedNoLongerApplicableFileList = string.Empty;

    public ObservableCollection<string> DabaseValidationErrors { get; } = new();

    [ObservableProperty]
    private string invalidFileList = string.Empty;

    [ObservableProperty]
    private string missingFilesList = string.Empty;

    [ObservableProperty]
    private string databaseExportDirectory = string.Empty;

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
    private void CreateDatabase()
    {
        var databasePath = configRepository.FilePaths.DatabasePath;

        if (File.Exists(databasePath))
        {
            dialogs.ShowErrorDialog(string.Format(Strings.ToolsCreateDatabaseFileAlreadyExists, databasePath));
            return;
        }

        if (dialogs.ShowConfirmDialog(string.Format(Strings.ToolsCreateDatabaseCreateDatabase, databasePath)))
        {
            try
            {
                DatabaseSetup.CreateDatabase(databasePath);
                dialogs.ShowInfoDialog($"Created database '{databasePath}'");
                Events.Send<CloseModalDialogRequest>();
            }
            catch (DatabaseWrapperException e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void CreateBackup()
    {
        try
        {
            new DatabaseBackup(fileSystem, configRepository).CreateBackup();
            dialogs.ShowInfoDialog("Backup created.");
            ScanBackupFiles();
        }
        catch (IOException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }

    private void ScanBackupFiles()
    {
        BackupFiles.Clear();

        if (Directory.Exists(configRepository.FilePaths.FilesRootDir))
        {
            foreach (var backupFile in new DatabaseBackup(fileSystem, configRepository).ListAvailableBackupFiles())
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
        dialogs.ShowInfoDialog($"Found {notApplicableFiles.Count} files that now should be filtered.");
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

        var filesValidator = new FileModelValidator();
        List<FileModel> invalidFiles = new();
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

        var resultText = DabaseValidationErrors.Count > 0 ? $"{DabaseValidationErrors.Count} errors found:" : $"No errors found.";
        dialogs.ShowInfoDialog(resultText);
        OnPropertyChanged(nameof(DabaseValidationErrors));
    }

    [RelayCommand]
    private void CopyInvalidFileList()
    {
        ClipboardService.SetText(InvalidFileList);
        dialogs.ShowInfoDialog("File list copied to clipboard.");
    }

    [RelayCommand]
    private void FileFinder()
    {
        List<FileModel> missingFiles = new();
        foreach (var file in filesystemAccessRepository.FilesystemAccess.GetFilesMissingInFilesystem(dbAccessRepository.DbAccess.GetFiles()))
        {
            missingFiles.Add(file);
        }

        var result = missingFiles.Count == 0 ? "No missing files found." : $"{missingFiles.Count} meta-data for missing files found.";
        dialogs.ShowInfoDialog(result);
        MissingFilesList = Utils.CreateFileList(missingFiles);
    }

    [RelayCommand]
    private void CopyFileFinderResult()
    {
        ClipboardService.SetText(MissingFilesList);
        dialogs.ShowInfoDialog("File list copied to clipboard.");
    }

    [RelayCommand]
    private void DatabaseExport()
    {
        if (!Directory.Exists(DatabaseExportDirectory))
        {
            dialogs.ShowErrorDialog("No such directory.");
            return;
        }

        if (Directory.GetFileSystemEntries(DatabaseExportDirectory).Length > 0)
        {
            dialogs.ShowErrorDialog("Specified directory is not empty.");
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
            dialogs.ShowInfoDialog($"Exported {persons.Count} persons, {locations.Count} locations, {tags.Count} tags and {files.Count} files.");
        }
        catch (Exception e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}
