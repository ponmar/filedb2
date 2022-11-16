using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Sorters;
using FileDB.Validators;
using FileDB.View;

namespace FileDB.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string configName = string.Empty;

    [ObservableProperty]
    private string database = string.Empty;

    [ObservableProperty]
    private string filesRootDirectory = string.Empty;

    [ObservableProperty]
    private int slideshowDelay;

    [ObservableProperty]
    private int searchHistorySize;

    [ObservableProperty]
    private SortMethod defaultSortMethod;

    public List<SortMethodDescription> SortMethods => Utils.GetSortMethods();

    [ObservableProperty]
    private bool keepSelectionAfterSort;

    [ObservableProperty]
    private bool includeHiddenDirectories;

    [ObservableProperty]
    private string blacklistedFilePathPatterns = string.Empty;

    [ObservableProperty]
    private string whitelistedFilePathPatterns = string.Empty;

    [ObservableProperty]
    private bool readOnly;

    [ObservableProperty]
    private bool backupReminder;

    [ObservableProperty]
    private bool birthdayReminder;

    [ObservableProperty]
    private bool birthdayReminderForDeceased;

    [ObservableProperty]
    private bool ripReminder;

    [ObservableProperty]
    private bool missingFilesRootDirNotification;

    [ObservableProperty]
    private string locationLink = string.Empty;

    [ObservableProperty]
    private int fileToLocationMaxDistance;

    [ObservableProperty]
    private WindowMode windowMode;

    [ObservableProperty]
    private string? castHttpServerInterface;

    [ObservableProperty]
    private int castHttpServerPort;

    [ObservableProperty]
    private bool cacheFiles;

    public List<WindowModeDescription> WindowModes => Utils.GetWindowModes();

    [ObservableProperty]
    int overlayTextSize;

    [ObservableProperty]
    int overlayTextSizeLarge;

    [RelayCommand]
    private void SetDefaultSlideshowDelay()
    {
        SlideshowDelay = DefaultConfigs.Default.SlideshowDelay;
    }

    [RelayCommand]
    private void SetDefaultBackupReminder()
    {
        BackupReminder = DefaultConfigs.Default.BackupReminder;
    }

    [RelayCommand]
    private void SetDefaultSearchHistorySize()
    {
        SearchHistorySize = DefaultConfigs.Default.SearchHistorySize;
    }

    [RelayCommand]
    private void SetDefaultDefaultSortMethod()
    {
        DefaultSortMethod = DefaultConfigs.Default.DefaultSortMethod;
    }

    [RelayCommand]
    private void SetDefaultCastHttpServerInterface()
    {
        CastHttpServerInterface = DefaultConfigs.Default.CastHttpServerInterface;
    }

    [RelayCommand]
    private void SetDefaultCastHttpServerPort()
    {
        CastHttpServerPort = DefaultConfigs.Default.CastHttpServerPort;
    }

    [RelayCommand]
    private void SetDefaultKeepSelectionAfterSort()
    {
        KeepSelectionAfterSort = DefaultConfigs.Default.KeepSelectionAfterSort;
    }

    [RelayCommand]
    private void SetDefaultBlacklistedFilePathPatterns()
    {
        BlacklistedFilePathPatterns = DefaultConfigs.Default.BlacklistedFilePathPatterns;
    }

    [RelayCommand]
    private void SetDefaultWhitelistedFilePathPatterns()
    {
        WhitelistedFilePathPatterns = DefaultConfigs.Default.WhitelistedFilePathPatterns;
    }

    [RelayCommand]
    private void SetDefaultIncludeHiddenDirectories()
    {
        IncludeHiddenDirectories = DefaultConfigs.Default.IncludeHiddenDirectories;
    }

    [RelayCommand]
    private void SetDefaultReadOnly()
    {
        ReadOnly = DefaultConfigs.Default.ReadOnly;
    }

    [RelayCommand]
    private void SetDefaultCacheFiles()
    {
        CacheFiles = DefaultConfigs.Default.CacheFiles;
    }

    [RelayCommand]
    private void SetDefaultBirthdayReminder()
    {
        BirthdayReminder = DefaultConfigs.Default.BirthdayReminder;
    }

    [RelayCommand]
    private void SetDefaultBirthdayReminderForDeceased()
    {
        BirthdayReminderForDeceased = DefaultConfigs.Default.BirthdayReminderForDeceased;
    }

    [RelayCommand]
    private void SetDefaultRipReminder()
    {
        RipReminder = DefaultConfigs.Default.RipReminder;
    }

    [RelayCommand]
    private void SetDefaultMissingFilesRootDirNotification()
    {
        MissingFilesRootDirNotification = DefaultConfigs.Default.MissingFilesRootDirNotification;
    }

    [RelayCommand]
    private void SetDefaultLocationLink()
    {
        LocationLink = DefaultConfigs.Default.LocationLink;
    }

    [RelayCommand]
    private void SetDefaultFileToLocationMaxDistance()
    {
        FileToLocationMaxDistance = DefaultConfigs.Default.FileToLocationMaxDistance;
    }

    [RelayCommand]
    private void SetDefaultWindowMode()
    {
        WindowMode = DefaultConfigs.Default.WindowMode;
    }

    [RelayCommand]
    private void SetDefaultOverlayTextSize()
    {
        OverlayTextSize = DefaultConfigs.Default.OverlayTextSize;
    }

    [RelayCommand]
    private void SetDefaultOverlayTextSizeLarge()
    {
        OverlayTextSizeLarge = DefaultConfigs.Default.OverlayTextSizeLarge;
    }

    private readonly Model.Model model = Model.Model.Instance;

    public SettingsViewModel()
    {
        UpdateFromConfiguration();
    }

    private void UpdateFromConfiguration()
    {
        ConfigName = model.Config.Name;
        Database = model.Config.Database;
        FilesRootDirectory = model.Config.FilesRootDirectory;
        SlideshowDelay = model.Config.SlideshowDelay;
        SearchHistorySize = model.Config.SearchHistorySize;
        DefaultSortMethod = model.Config.DefaultSortMethod;
        KeepSelectionAfterSort = model.Config.KeepSelectionAfterSort;
        IncludeHiddenDirectories = model.Config.IncludeHiddenDirectories;
        BlacklistedFilePathPatterns = model.Config.BlacklistedFilePathPatterns;
        WhitelistedFilePathPatterns = model.Config.WhitelistedFilePathPatterns;
        ReadOnly = model.Config.ReadOnly;
        BackupReminder = model.Config.BackupReminder;
        BirthdayReminder = model.Config.BirthdayReminder;
        BirthdayReminderForDeceased = model.Config.BirthdayReminderForDeceased;
        RipReminder = model.Config.RipReminder;
        MissingFilesRootDirNotification = model.Config.MissingFilesRootDirNotification;
        LocationLink = model.Config.LocationLink;
        FileToLocationMaxDistance = model.Config.FileToLocationMaxDistance;
        WindowMode = model.Config.WindowMode;
        CastHttpServerInterface = model.Config.CastHttpServerInterface;
        CastHttpServerPort = model.Config.CastHttpServerPort;
        CacheFiles = model.Config.CacheFiles;
        OverlayTextSize = model.Config.OverlayTextSize;
        OverlayTextSizeLarge = model.Config.OverlayTextSizeLarge;
    }

    [RelayCommand]
    private void ResetConfiguration()
    {
        UpdateFromConfiguration();
    }

    [RelayCommand]
    private void SaveConfiguration()
    {
        var config = new Config(
            ConfigName,
            Database,
            FilesRootDirectory,
            fileToLocationMaxDistance,
            BlacklistedFilePathPatterns,
            WhitelistedFilePathPatterns,
            IncludeHiddenDirectories,
            SlideshowDelay,
            SearchHistorySize,
            DefaultSortMethod,
            KeepSelectionAfterSort,
            ReadOnly,
            BackupReminder,
            BirthdayReminder,
            BirthdayReminderForDeceased,
            RipReminder,
            MissingFilesRootDirNotification,
            LocationLink,
            WindowMode,
            CastHttpServerInterface,
            CastHttpServerPort,
            CacheFiles,
            OverlayTextSize,
            OverlayTextSizeLarge);

        var result = new ConfigValidator().Validate(config);
        if (!result.IsValid)
        {
            Dialogs.Instance.ShowErrorDialog(result);
            return;
        }

        var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName);

        if (!Dialogs.Instance.ShowConfirmDialog($"Write your configuration to {appDataConfig.FilePath}?"))
        {
            return;
        }

        Utils.BackupFile(appDataConfig.FilePath);

        if (appDataConfig.Write(config))
        {
            model.Config = config;
            model.ReloadHandles();
        }
        else
        {
            Dialogs.Instance.ShowErrorDialog("Unable to save configuration");
        }
    }

    [RelayCommand]
    private void BrowseDatabase()
    {
        var result = Dialogs.Instance.BrowseExistingFileDialog(@"c:\", $"{Utils.ApplicationName} database files (*.db)|*.db");
        if (result != null)
        {
            Database = result;
        }
    }

    [RelayCommand]
    private void BrowseFilesRootDirectory()
    {
        var result = Dialogs.Instance.BrowseExistingDirectory(@"c:\", "Select your files root directory");
        if (result != null)
        {
            FilesRootDirectory = result;
        }
    }

    [RelayCommand]
    public void CreateDatabase()
    {
        var createdDatabasePath = Dialogs.Instance.ShowCreateDatabaseDialog();
        if (createdDatabasePath != null)
        {
            Database = createdDatabasePath;
        }
    }
}
