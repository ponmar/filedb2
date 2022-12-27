using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Export;
using FileDBShared.Model;
using FileDBShared.Validators;
using TextCopy;

namespace FileDB.ViewModel;

public partial class ToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private string backupResult = string.Empty;

    [ObservableProperty]
    private string cacheResult = "Not executed.";

    public ObservableCollection<BackupFile> BackupFiles { get; } = new();

    [ObservableProperty]
    private string findImportedNoLongerApplicableFilesResult = "Not executed.";

    [ObservableProperty]
    private string importedNoLongerApplicableFileList = string.Empty;

    [ObservableProperty]
    private string databaseValidationResult = "Not executed.";

    public ObservableCollection<string> DabaseValidationErrors { get; } = new();

    [ObservableProperty]
    private string invalidFileList = string.Empty;

    [ObservableProperty]
    private string fileFinderResult = "Not executed.";

    [ObservableProperty]
    private string missingFilesList = string.Empty;

    [ObservableProperty]
    private string databaseExportDirectory = string.Empty;

    [ObservableProperty]
    private string databaseExportResult = "Not executed.";

    private readonly Model.Model model = Model.Model.Instance;

    public ToolsViewModel()
    {
        ScanBackupFiles();
    }

    [RelayCommand]
    private void CreateBackup()
    {
        try
        {
            new DatabaseBackup().CreateBackup();
            ScanBackupFiles();
        }
        catch (IOException e)
        {
            Dialogs.Instance.ShowErrorDialog(e.Message);
        }
    }

    [RelayCommand]
    private void OpenDatabaseBackupDirectory()
    {
        Utils.OpenDirectoryInExplorer(new DatabaseBackup().BackupDirectory);
    }

    [RelayCommand]
    private void CreateCache()
    {
        CacheResult = "Running...";

        var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
        var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);
        if (!Directory.Exists(cacheDir))
        {
            try
            {
                Directory.CreateDirectory(cacheDir);
            }
            catch (Exception e)
            {
                CacheResult = $"Cache creation error: {e.Message}";
                return;
            }
        }

        var profileFileIds = model.DbAccess.GetPersons().Where(x => x.ProfileFileId != null).Select(x => x.ProfileFileId!.Value);
        var numCachedFiles = 0;
        foreach (var profileFileId in profileFileIds)
        {
            var file = model.DbAccess.GetFileById(profileFileId)!;
            var sourcePath = model.FilesystemAccess.ToAbsolutePath(file.Path);
            var destinationPath = Path.Combine(cacheDir, $"{profileFileId}");
            try
            {
                if (!File.Exists(destinationPath))
                {
                    File.Copy(sourcePath, destinationPath);
                    numCachedFiles++;
                }
            }
            catch (Exception e)
            {
                CacheResult = $"Unable to cache file: {e.Message}";
                return;
            }
        }

        CacheResult = $"Cached {numCachedFiles} files.";
    }

    [RelayCommand]
    private void OpenCacheDirectory()
    {
        var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
        var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);
        var dirToOpen = Directory.Exists(cacheDir) ? cacheDir : configDir;
        Utils.OpenDirectoryInExplorer(dirToOpen);
    }

    private void ScanBackupFiles()
    {
        BackupFiles.Clear();

        var backupHandler = new DatabaseBackup();

        if (Directory.Exists(backupHandler.BackupDirectory))
        {
            foreach (var backupFile in backupHandler.ListAvailableBackupFiles())
            {
                BackupFiles.Add(backupFile);
            }

            BackupResult = BackupFiles.Count > 0 ? $"{BackupFiles.Count} database backup files found:" : $"No database backup files found!";
        }
        else
        {
            BackupResult = "Directory for configured database does not exist.";
        }

        OnPropertyChanged(nameof(BackupResult));
    }

    [RelayCommand]
    private void FindImportedNoLongerApplicableFiles()
    {
        var blacklistedFilePathPatterns = model.Config.BlacklistedFilePathPatterns.Split(";");
        var whitelistedFilePathPatterns = model.Config.WhitelistedFilePathPatterns.Split(";");
        var notApplicableFiles = model.DbAccess.GetFiles().Where(x => !model.FilesystemAccess.PathIsApplicable(x.Path, blacklistedFilePathPatterns, whitelistedFilePathPatterns, model.Config.IncludeHiddenDirectories)).ToList();
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
        foreach (var file in model.DbAccess.GetFiles())
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
        foreach (var person in model.DbAccess.GetPersons())
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
        foreach (var location in model.DbAccess.GetLocations())
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
        foreach (var tag in model.DbAccess.GetTags())
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
        foreach (var file in model.FilesystemAccess.GetFilesMissingInFilesystem(model.DbAccess.GetFiles()))
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
            var persons = model.DbAccess.GetPersons().ToList();
            var locations = model.DbAccess.GetLocations().ToList();
            var tags = model.DbAccess.GetTags().ToList();
            var files = model.DbAccess.GetFiles().ToList();
            exporter.Export(persons, locations, tags, files);
            DatabaseExportResult = $"Exported {persons.Count} persons, {locations.Count} locations, {tags.Count} tags and {files.Count} files.";
        }
        catch (Exception e)
        {
            DatabaseExportResult = e.Message;
        }
    }
}
