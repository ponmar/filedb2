using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBAvalonia.Validators;

namespace FileDBAvalonia.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isDirty;

    partial void OnIsDirtyChanged(bool value)
    {
        Messenger.Send(new ConfigEdited(value));
    }

    [ObservableProperty]
    private int slideshowDelay;

    partial void OnSlideshowDelayChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private int searchHistorySize;

    partial void OnSearchHistorySizeChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private SortMethod defaultSortMethod;

    partial void OnDefaultSortMethodChanged(SortMethod value) => IsDirty = true;

    [ObservableProperty]
    private List<SortMethod> sortMethods = Enum.GetValues<SortMethod>().ToList();

    [ObservableProperty]
    private bool keepSelectionAfterSort;

    partial void OnKeepSelectionAfterSortChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool includeHiddenDirectories;

    partial void OnIncludeHiddenDirectoriesChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private string blacklistedFilePathPatterns = string.Empty;

    partial void OnBlacklistedFilePathPatternsChanged(string value) => IsDirty = true;

    [ObservableProperty]
    private string whitelistedFilePathPatterns = string.Empty;

    partial void OnWhitelistedFilePathPatternsChanged(string value) => IsDirty = true;

    [ObservableProperty]
    private bool readOnly;

    partial void OnReadOnlyChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool backupReminder;

    partial void OnBackupReminderChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool birthdayReminder;

    partial void OnBirthdayReminderChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool birthdayReminderForDeceased;

    partial void OnBirthdayReminderForDeceasedChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool ripReminder;

    partial void OnRipReminderChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private bool missingFilesRootDirNotification;

    partial void OnMissingFilesRootDirNotificationChanged(bool value) => IsDirty = true;

    [ObservableProperty]
    private string locationLink = string.Empty;

    partial void OnLocationLinkChanged(string value) => IsDirty = true;

    [ObservableProperty]
    private int fileToLocationMaxDistance;

    partial void OnFileToLocationMaxDistanceChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private WindowMode windowMode;

    partial void OnWindowModeChanged(WindowMode value) => IsDirty = true;

    [ObservableProperty]
    private ObservableCollection<CultureInfo> languages = [];

    [ObservableProperty]
    private CultureInfo? selectedLanguage;

    partial void OnSelectedLanguageChanged(CultureInfo? value) => IsDirty = true;

    [ObservableProperty]
    public List<WindowMode> windowModes = Enum.GetValues<WindowMode>().ToList();

    [ObservableProperty]
    private int imageMemoryCacheCount;

    partial void OnImageMemoryCacheCountChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private int numImagesToPreload;

    partial void OnNumImagesToPreloadChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private int overlayTextSize;

    partial void OnOverlayTextSizeChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private int overlayTextSizeLarge;

    partial void OnOverlayTextSizeLargeChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private int shortItemNameMaxLength;

    partial void OnShortItemNameMaxLengthChanged(int value) => IsDirty = true;

    [ObservableProperty]
    private List<Theme> themes = Enum.GetValues<Theme>().ToList();

    [ObservableProperty]
    private Theme theme;

    partial void OnThemeChanged(Theme value)
    {
        IsDirty = true;
        Messenger.Send(new SetTheme(value));
    }

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
        SelectedLanguage = Languages.FirstOrDefault(x => x.Name == DefaultConfigs.Default.Language);
    }

    [RelayCommand]
    private void SetDefaultTheme()
    {
        Theme = DefaultConfigs.Default.Theme;
    }

    private readonly IConfigProvider configProvider;
    private readonly IConfigUpdater configUpdater;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;
    private readonly INotificationHandling notificationHandling;

    public SettingsViewModel(IConfigProvider configProvider, IConfigUpdater configUpdater, IDialogs dialogs, IFileSystem fileSystem, INotificationHandling notificationHandling)
    {
        this.configProvider = configProvider;
        this.configUpdater = configUpdater;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;
        this.notificationHandling = notificationHandling;

        languages.Add(CultureInfo.GetCultureInfo("en"));
        languages.Add(CultureInfo.GetCultureInfo("sv-SE"));

        UpdateFromConfiguration();
    }

    private void UpdateFromConfiguration()
    {
        var config = configProvider.Config;

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
        SelectedLanguage = Languages.FirstOrDefault(x => x.Name == config.Language);
        Theme = config.Theme;

        IsDirty = false;
    }

    [RelayCommand]
    private void ResetConfiguration()
    {
        UpdateFromConfiguration();
    }

    [RelayCommand]
    private async Task SaveConfigurationAsync()
    {
        var configToSave = new Config(
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
            SelectedLanguage?.Name == "en" ? null : SelectedLanguage?.Name,
            Theme);

        var result = new ConfigValidator().Validate(configToSave);
        if (!result.IsValid)
        {
            await dialogs.ShowErrorDialogAsync(result);
            return;
        }

        if (!await dialogs.ShowConfirmDialogAsync($"Save configuration?"))
        {
            return;
        }

        try
        {
            var configPath = configProvider.FilePaths.ConfigPath;
            new FileBackup(fileSystem, configPath).CreateBackup();
            var json = configToSave.ToJson();
            fileSystem.File.WriteAllText(configPath, json);

            configUpdater.UpdateConfig(configToSave);

            IsDirty = false;
        }
        catch (Exception e)
        {
            await dialogs.ShowErrorDialogAsync("Unable to save configuration", e);
        }
    }
}
