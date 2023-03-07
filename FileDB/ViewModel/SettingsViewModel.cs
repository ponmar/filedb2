using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Sorters;
using FileDB.Validators;

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
    private bool cacheFiles;

    [ObservableProperty]
    private ObservableCollection<CultureInfo> cultures = new();

    [ObservableProperty]
    private CultureInfo? selectedCulture;

    public List<WindowModeDescription> WindowModes => Utils.GetWindowModes();

    [ObservableProperty]
    int overlayTextSize;

    [ObservableProperty]
    int overlayTextSizeLarge;

    [ObservableProperty]
    int shortItemNameMaxLength;

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

    [RelayCommand]
    private void SetDefaultShortItemNameMaxLength()
    {
        ShortItemNameMaxLength = DefaultConfigs.Default.ShortItemNameMaxLength;
    }

    [RelayCommand]
    private void SetDefaultCultureOverride()
    {
        SelectedCulture = Cultures.FirstOrDefault(x => x.Name == DefaultConfigs.Default.CultureOverride);
    }

    private Config config;
    private readonly IConfigSaver configSaver;

    public SettingsViewModel(Config config, IConfigSaver configSaver)
    {
        this.config = config;
        this.configSaver = configSaver;

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            cultures.Add(culture);
        }

        UpdateFromConfiguration();
    }

    private void UpdateFromConfiguration()
    {
        ConfigName = config.Name;
        Database = config.Database;
        FilesRootDirectory = config.FilesRootDirectory;
        SlideshowDelay = config.SlideshowDelay;
        SearchHistorySize = config.SearchHistorySize;
        DefaultSortMethod = config.DefaultSortMethod;
        KeepSelectionAfterSort = config.KeepSelectionAfterSort;
        IncludeHiddenDirectories = config.IncludeHiddenDirectories;
        BlacklistedFilePathPatterns = config.BlacklistedFilePathPatterns;
        WhitelistedFilePathPatterns = config.WhitelistedFilePathPatterns;
        ReadOnly = config.ReadOnly;
        BackupReminder = config.BackupReminder;
        BirthdayReminder = config.BirthdayReminder;
        BirthdayReminderForDeceased = config.BirthdayReminderForDeceased;
        RipReminder = config.RipReminder;
        MissingFilesRootDirNotification = config.MissingFilesRootDirNotification;
        LocationLink = config.LocationLink;
        FileToLocationMaxDistance = config.FileToLocationMaxDistance;
        WindowMode = config.WindowMode;
        CacheFiles = config.CacheFiles;
        OverlayTextSize = config.OverlayTextSize;
        OverlayTextSizeLarge = config.OverlayTextSizeLarge;
        ShortItemNameMaxLength = config.ShortItemNameMaxLength;
        SelectedCulture = Cultures.FirstOrDefault(x => x.Name == config.CultureOverride);
    }

    [RelayCommand]
    private void ResetConfiguration()
    {
        UpdateFromConfiguration();
    }

    [RelayCommand]
    private void SaveConfiguration()
    {
        var configToSave = new Config(
            ConfigName,
            Database,
            FilesRootDirectory,
            FileToLocationMaxDistance,
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
            CacheFiles,
            OverlayTextSize,
            OverlayTextSizeLarge,
            ShortItemNameMaxLength,
            SelectedCulture?.Name);

        var result = new ConfigValidator().Validate(configToSave);
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

        if (appDataConfig.Write(configToSave))
        {
            configSaver.UpdateConfig(configToSave);
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
