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
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using FileDBShared.Validators;
using TextCopy;

namespace FileDB.ViewModel;

public partial class ToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private string backupListHeader = string.Empty;

    public ObservableCollection<BackupFile> BackupFiles { get; } = [];

    [ObservableProperty]
    private string importedNoLongerApplicableFileList = string.Empty;

    public ObservableCollection<string> DabaseValidationErrors { get; } = [];

    [ObservableProperty]
    private string invalidFileList = string.Empty;

    [ObservableProperty]
    private string missingFilesList = string.Empty;

    [ObservableProperty]
    private string databaseExportDirectory = string.Empty;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;

    public ToolsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IDialogs dialogs, IFileSystem fileSystem)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;
        ScanBackupFiles();
    }

    [RelayCommand]
    private void CreateDatabase()
    {
        var databasePath = configProvider.FilePaths.DatabasePath;

        if (fileSystem.File.Exists(databasePath))
        {
            dialogs.ShowErrorDialog(string.Format(Strings.ToolsCreateDatabaseFileAlreadyExists, databasePath));
            return;
        }

        if (dialogs.ShowConfirmDialog(string.Format(Strings.ToolsCreateDatabaseCreateDatabase, databasePath)))
        {
            try
            {
                DatabaseSetup.CreateDatabase(databasePath);
                dialogs.ShowInfoDialog(string.Format(Strings.ToolsCreateDatabaseCreated, databasePath));
                Events.Send<CloseModalDialogRequest>();
            }
            catch (Exception e)
            {
                dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void CreateDatabaseBackup()
    {
        try
        {
            new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).CreateBackup();
            dialogs.ShowInfoDialog(Strings.ToolsCreateBackupResult);
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

        if (fileSystem.Directory.Exists(configProvider.FilePaths.FilesRootDir))
        {
            foreach (var backupFile in new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).ListAvailableBackupFiles())
            {
                BackupFiles.Add(backupFile);
            }

            BackupListHeader = BackupFiles.Count > 0 ? Strings.ToolsDatabaseBackupFilesLabel : Strings.ToolsDatabaseBackupNoFilesFound;
        }
        else
        {
            BackupListHeader = Strings.ToolsBackupDatabaseDirectoryDoesNotExist;
        }
    }

    [RelayCommand]
    private void FindImportedNoLongerApplicableFiles()
    {
        var blacklistedFilePathPatterns = configProvider.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configProvider.Config.WhitelistedFilePathPatterns.Split(";");
        var notApplicableFiles = dbAccessProvider.DbAccess.GetFiles().Where(x => !filesystemAccessProvider.FilesystemAccess.PathIsApplicable(x.Path, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configProvider.Config.IncludeHiddenDirectories)).ToList();
        ImportedNoLongerApplicableFileList = Utils.CreateFileList(notApplicableFiles);
        dialogs.ShowInfoDialog(string.Format(Strings.ToolsFoundNotApplicableFilesCountFilesThatNowShouldBeFiltered, notApplicableFiles.Count));
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
        List<FileModel> invalidFiles = [];
        foreach (var file in dbAccessProvider.DbAccess.GetFiles())
        {
            var result = filesValidator.Validate(file);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add(string.Format(Strings.ToolsDatabaseValidationFileError, file.Id, error.ErrorMessage));
                }
                invalidFiles.Add(file);
            }
        }
        InvalidFileList = Utils.CreateFileList(invalidFiles);

        var personValidator = new PersonModelValidator();
        foreach (var person in dbAccessProvider.DbAccess.GetPersons())
        {
            var result = personValidator.Validate(person);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add(string.Format(Strings.ToolsDatabaseValidationPersonError, person.Id, error.ErrorMessage));
                }
            }
        }

        var locationValidator = new LocationModelValidator();
        foreach (var location in dbAccessProvider.DbAccess.GetLocations())
        {
            var result = locationValidator.Validate(location);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add(string.Format(Strings.ToolsDatabaseValidationLocationError, location.Id, error.ErrorMessage));
                }
            }
        }

        var tagValidator = new TagModelValidator();
        foreach (var tag in dbAccessProvider.DbAccess.GetTags())
        {
            var result = tagValidator.Validate(tag);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    DabaseValidationErrors.Add(string.Format(Strings.ToolsDatabaseValidationTagError, tag.Id, error.ErrorMessage));
                }
            }
        }

        var resultText = DabaseValidationErrors.Count > 0 ? string.Format(Strings.ToolsDabaseValidationErrorsFound, DabaseValidationErrors.Count) : Strings.ToolsDatabaseValidationNoErrorsFound;
        dialogs.ShowInfoDialog(resultText);
        OnPropertyChanged(nameof(DabaseValidationErrors));
    }

    [RelayCommand]
    private void CopyInvalidFileList()
    {
        ClipboardService.SetText(InvalidFileList);
        dialogs.ShowInfoDialog(Strings.ToolsFileListCopiedToClipboard);
    }

    [RelayCommand]
    private void FileFinder()
    {
        List<FileModel> missingFiles = [.. filesystemAccessProvider.FilesystemAccess.GetFilesMissingInFilesystem(dbAccessProvider.DbAccess.GetFiles())];

        var result = missingFiles.Count == 0 ? Strings.ToolsNoMissingFilesFound : string.Format(Strings.ToolsMissingFilesFound, missingFiles.Count);
        dialogs.ShowInfoDialog(result);
        MissingFilesList = Utils.CreateFileList(missingFiles);
    }

    [RelayCommand]
    private void CopyFileFinderResult()
    {
        ClipboardService.SetText(MissingFilesList);
        dialogs.ShowInfoDialog(Strings.ToolsFileListCopiedToClipboard);
    }

    [RelayCommand]
    private void BrowseDatabaseExportDirectory()
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        DatabaseExportDirectory = dialogs.BrowseExistingDirectory(initialPath, "Select your destination directory") ?? string.Empty;
    }

    [RelayCommand]
    private void DatabaseExport()
    {
        if (!fileSystem.Directory.Exists(DatabaseExportDirectory))
        {
            dialogs.ShowErrorDialog(Strings.ToolsDatabaseExportNoSuchDirectory);
            return;
        }

        if (fileSystem.Directory.GetFileSystemEntries(DatabaseExportDirectory).Length > 0)
        {
            dialogs.ShowErrorDialog(Strings.ToolsDatabaseExportSpecifiedDirectoryIsNotEmpty);
            return;
        }

        try
        {
            var exporter = new DatabaseExportHandler(DatabaseExportDirectory, fileSystem);
            var persons = dbAccessProvider.DbAccess.GetPersons().ToList();
            var locations = dbAccessProvider.DbAccess.GetLocations().ToList();
            var tags = dbAccessProvider.DbAccess.GetTags().ToList();
            var files = dbAccessProvider.DbAccess.GetFiles().ToList();
            exporter.Export(persons, locations, tags, files);
            dialogs.ShowInfoDialog(string.Format(Strings.ToolsDatabaseExportResult, persons.Count, locations.Count, tags.Count, files.Count));
        }
        catch (Exception e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}
