using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Model;
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

    [ObservableProperty]
    private List<SortMethod> sortMethods = Enum.GetValues<SortMethod>().ToList();

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
    private ObservableCollection<CultureInfo> cultures = new();

    [ObservableProperty]
    private CultureInfo? selectedCulture;

    partial void OnSelectedCultureChanged(CultureInfo? value)
    {
        if (value is not null)
        {
            Utils.SetUICulture(value);
        }
        else
        {
            Utils.SetInvariantUICulture();
        }
    }

    [ObservableProperty]
    public List<WindowMode> windowModes = Enum.GetValues<WindowMode>().ToList();

    [ObservableProperty]
    private int imageMemoryCacheCount;

    [ObservableProperty]
    private int numImagesToPreload;

    [ObservableProperty]
    private int overlayTextSize;

    [ObservableProperty]
    private int overlayTextSizeLarge;

    [ObservableProperty]
    private int shortItemNameMaxLength;

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
    private void SetDefaultImageMemoryCacheCount()
    {
        ImageMemoryCacheCount = DefaultConfigs.Default.ImageMemoryCacheCount;
    }

    [RelayCommand]
    private void SetDefaultNumImagesToPreload()
    {
        NumImagesToPreload = DefaultConfigs.Default.NumImagesToPreload;
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
    private void SetDefaultLanguage()
    {
        SelectedCulture = Cultures.FirstOrDefault(x => x.Name == DefaultConfigs.Default.Language);
    }

    private readonly IConfigRepository configRepository;
    private readonly IConfigUpdater configUpdater;
    private readonly IDialogs dialogs;

    public SettingsViewModel(IConfigRepository configRepository, IConfigUpdater configUpdater, IDialogs dialogs)
    {
        this.configRepository = configRepository;
        this.configUpdater = configUpdater;
        this.dialogs = dialogs;

        cultures.Add(CultureInfo.GetCultureInfo("en"));
        cultures.Add(CultureInfo.GetCultureInfo("sv-SE"));

        UpdateFromConfiguration();
    }

    private void UpdateFromConfiguration()
    {
        var config = configRepository.Config;

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
        ImageMemoryCacheCount = config.ImageMemoryCacheCount;
        NumImagesToPreload = config.NumImagesToPreload;
        OverlayTextSize = config.OverlayTextSize;
        OverlayTextSizeLarge = config.OverlayTextSizeLarge;
        ShortItemNameMaxLength = config.ShortItemNameMaxLength;
        SelectedCulture = Cultures.FirstOrDefault(x => x.Name == config.Language);
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
            ImageMemoryCacheCount,
            NumImagesToPreload,
            OverlayTextSize,
            OverlayTextSizeLarge,
            ShortItemNameMaxLength,
            SelectedCulture?.Name);

        var result = new ConfigValidator().Validate(configToSave);
        if (!result.IsValid)
        {
            dialogs.ShowErrorDialog(result);
            return;
        }

        var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName, ServiceLocator.Resolve<IFileSystem>());

        if (!dialogs.ShowConfirmDialog($"Write your configuration to {appDataConfig.FilePath}?"))
        {
            return;
        }

        Utils.BackupFile(appDataConfig.FilePath);

        if (appDataConfig.Write(configToSave))
        {
            configUpdater.UpdateConfig(configToSave);
        }
        else
        {
            dialogs.ShowErrorDialog("Unable to save configuration");
        }
    }

    [RelayCommand]
    private void BrowseDatabase()
    {
        var result = dialogs.BrowseExistingFileDialog(@"c:\", $"{Utils.ApplicationName} database files (*.db)|*.db");
        if (result != null)
        {
            Database = result;
        }
    }

    [RelayCommand]
    private void BrowseFilesRootDirectory()
    {
        var result = dialogs.BrowseExistingDirectory(@"c:\", "Select your files root directory");
        if (result != null)
        {
            FilesRootDirectory = result;
        }
    }

    [RelayCommand]
    public void CreateDatabase()
    {
        var createdDatabasePath = dialogs.ShowCreateDatabaseDialog();
        if (createdDatabasePath != null)
        {
            Database = createdDatabasePath;
        }
    }
}
