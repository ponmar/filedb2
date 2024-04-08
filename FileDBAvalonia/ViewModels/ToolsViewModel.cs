using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Export;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using FileDBShared.Validators;

namespace FileDBAvalonia.ViewModels;

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
    private readonly IClipboardService clipboardService;

    public ToolsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IDialogs dialogs, IFileSystem fileSystem, IClipboardService clipboardService)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;
        this.clipboardService = clipboardService;
        ScanBackupFiles();
    }

    [RelayCommand]
    private async Task CreateDatabaseAsync()
    {
        var databasePath = configProvider.FilePaths.DatabasePath;

        if (fileSystem.File.Exists(databasePath))
        {
            await dialogs.ShowErrorDialogAsync(string.Format(Strings.ToolsCreateDatabaseFileAlreadyExists, databasePath));
            return;
        }

        if (await dialogs.ShowConfirmDialogAsync(string.Format(Strings.ToolsCreateDatabaseCreateDatabase, databasePath)))
        {
            try
            {
                DatabaseSetup.CreateDatabase(databasePath);
                await dialogs.ShowInfoDialogAsync(string.Format(Strings.ToolsCreateDatabaseCreated, databasePath));
                Messenger.Send<CloseModalDialogRequest>();
            }
            catch (Exception e)
            {
                await dialogs.ShowErrorDialogAsync(e.Message);
            }
        }
    }

    [RelayCommand]
    private async Task CreateDatabaseBackupAsync()
    {
        try
        {
            new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath).CreateBackup();
            await dialogs.ShowInfoDialogAsync(Strings.ToolsCreateBackupResult);
            ScanBackupFiles();
        }
        catch (IOException e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
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
    private async Task FindImportedNoLongerApplicableFilesAsync()
    {
        var blacklistedFilePathPatterns = configProvider.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = configProvider.Config.WhitelistedFilePathPatterns.Split(";");
        var notApplicableFiles = dbAccessProvider.DbAccess.GetFiles().Where(x => !filesystemAccessProvider.FilesystemAccess.PathIsApplicable(x.Path, blacklistedFilePathPatterns, whitelistedFilePathPatterns, configProvider.Config.IncludeHiddenDirectories)).ToList();
        ImportedNoLongerApplicableFileList = Utils.CreateFileList(notApplicableFiles);
        await dialogs.ShowInfoDialogAsync(string.Format(Strings.ToolsFoundNotApplicableFilesCountFilesThatNowShouldBeFiltered, notApplicableFiles.Count));
    }

    [RelayCommand]
    private void CopyImportedNoLongerApplicableFilesList()
    {
        clipboardService.SetTextAsync(ImportedNoLongerApplicableFileList);
    }

    [RelayCommand]
    private async Task DatabaseValidationAsync()
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
        await dialogs.ShowInfoDialogAsync(resultText);
        OnPropertyChanged(nameof(DabaseValidationErrors));
    }

    [RelayCommand]
    private async Task CopyInvalidFileListAsync()
    {
        await clipboardService.SetTextAsync(InvalidFileList);
        await dialogs.ShowInfoDialogAsync(Strings.ToolsFileListCopiedToClipboard);
    }

    [RelayCommand]
    private async Task FileFinderAsync()
    {
        List<FileModel> missingFiles = [.. filesystemAccessProvider.FilesystemAccess.GetFilesMissingInFilesystem(dbAccessProvider.DbAccess.GetFiles())];

        var result = missingFiles.Count == 0 ? Strings.ToolsNoMissingFilesFound : string.Format(Strings.ToolsMissingFilesFound, missingFiles.Count);
        await dialogs.ShowInfoDialogAsync(result);
        MissingFilesList = Utils.CreateFileList(missingFiles);
    }

    [RelayCommand]
    private async Task CopyFileFinderResultAsync()
    {
        await clipboardService.SetTextAsync(MissingFilesList);
        await dialogs.ShowInfoDialogAsync(Strings.ToolsFileListCopiedToClipboard);
    }

    [RelayCommand]
    private async Task BrowseDatabaseExportDirectoryAsync()
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var selectedDir = await dialogs.ShowBrowseExistingDirectoryDialogAsync("Select your destination directory", initialPath);
        DatabaseExportDirectory = selectedDir ?? string.Empty;
    }

    [RelayCommand]
    private async Task DatabaseExportAsync()
    {
        if (!fileSystem.Directory.Exists(DatabaseExportDirectory))
        {
            await dialogs.ShowErrorDialogAsync(Strings.ToolsDatabaseExportNoSuchDirectory);
            return;
        }

        if (fileSystem.Directory.GetFileSystemEntries(DatabaseExportDirectory).Length > 0)
        {
            await dialogs.ShowErrorDialogAsync(Strings.ToolsDatabaseExportSpecifiedDirectoryIsNotEmpty);
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
            await dialogs.ShowInfoDialogAsync(string.Format(Strings.ToolsDatabaseExportResult, persons.Count, locations.Count, tags.Count, files.Count));
        }
        catch (Exception e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
        }
    }
}
