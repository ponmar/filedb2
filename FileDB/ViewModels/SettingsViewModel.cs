using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Dialogs;
using FileDB.Extensions;
using FileDB.Model;
using FileDB.Validators;

namespace FileDB.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public static IEnumerable<SortMethod> SortMethods { get; } = Enum.GetValues<SortMethod>();
    public static IEnumerable<WindowMode> WindowModes { get; } = Enum.GetValues<WindowMode>();
    public static IEnumerable<Theme> Themes { get; } = Enum.GetValues<Theme>();
    public static IEnumerable<CultureInfo> Languages { get; } = [CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("sv-SE")];
    public static IEnumerable<FilterType> SearchFilterTypes { get; } = Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString(), StringComparer.Ordinal);

    [ObservableProperty]
    private bool isDirty;

    partial void OnIsDirtyChanged(bool value) => Messenger.Send(new ConfigEdited(value));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlideshowDelayIsDefault))]
    private int slideshowDelay;

    partial void OnSlideshowDelayChanged(int value) => IsDirty = true;

    public bool SlideshowDelayIsDefault => SlideshowDelay == DefaultConfigs.Default.SlideshowDelay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchHistorySizeIsDefault))]
    private int searchHistorySize;

    partial void OnSearchHistorySizeChanged(int value) => IsDirty = true;

    public bool SearchHistorySizeIsDefault => SearchHistorySize == DefaultConfigs.Default.SearchHistorySize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultSortMethodIsDefault))]
    private SortMethod defaultSortMethod;

    partial void OnDefaultSortMethodChanged(SortMethod value) => IsDirty = true;

    public bool DefaultSortMethodIsDefault => DefaultSortMethod == DefaultConfigs.Default.DefaultSortMethod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KeepSelectionAfterSortIsDefault))]
    private bool keepSelectionAfterSort;

    partial void OnKeepSelectionAfterSortChanged(bool value) => IsDirty = true;

    public bool KeepSelectionAfterSortIsDefault => KeepSelectionAfterSort == DefaultConfigs.Default.KeepSelectionAfterSort;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncludeHiddenDirectoriesIsDefault))]
    private bool includeHiddenDirectories;

    partial void OnIncludeHiddenDirectoriesChanged(bool value) => IsDirty = true;

    public bool IncludeHiddenDirectoriesIsDefault => IncludeHiddenDirectories == DefaultConfigs.Default.IncludeHiddenDirectories;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BlacklistedFilePathPatternsIsDefault))]
    private string blacklistedFilePathPatterns = string.Empty;

    partial void OnBlacklistedFilePathPatternsChanged(string value) => IsDirty = true;

    public bool BlacklistedFilePathPatternsIsDefault => BlacklistedFilePathPatterns == DefaultConfigs.Default.BlacklistedFilePathPatterns;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WhitelistedFilePathPatternsIsDefault))]
    private string whitelistedFilePathPatterns = string.Empty;

    partial void OnWhitelistedFilePathPatternsChanged(string value) => IsDirty = true;

    public bool WhitelistedFilePathPatternsIsDefault => WhitelistedFilePathPatterns == DefaultConfigs.Default.WhitelistedFilePathPatterns;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReadOnlyIsDefault))]
    private bool readOnly;

    partial void OnReadOnlyChanged(bool value) => IsDirty = true;

    public bool ReadOnlyIsDefault => ReadOnly == DefaultConfigs.Default.ReadOnly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackupReminderIsDefault))]
    private bool backupReminder;

    partial void OnBackupReminderChanged(bool value) => IsDirty = true;

    public bool BackupReminderIsDefault => BackupReminder == DefaultConfigs.Default.BackupReminder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BirthdayReminderIsDefault))]
    private bool birthdayReminder;

    partial void OnBirthdayReminderChanged(bool value) => IsDirty = true;

    public bool BirthdayReminderIsDefault => BirthdayReminder == DefaultConfigs.Default.BirthdayReminder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BirthdayReminderForDeceasedIsDefault))]
    private bool birthdayReminderForDeceased;

    partial void OnBirthdayReminderForDeceasedChanged(bool value) => IsDirty = true;

    public bool BirthdayReminderForDeceasedIsDefault => BirthdayReminderForDeceased == DefaultConfigs.Default.BirthdayReminderForDeceased;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RipReminderIsDefault))]
    private bool ripReminder;

    partial void OnRipReminderChanged(bool value) => IsDirty = true;

    public bool RipReminderIsDefault => RipReminder == DefaultConfigs.Default.RipReminder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MissingFilesRootDirNotificationIsDefault))]
    private bool missingFilesRootDirNotification;

    partial void OnMissingFilesRootDirNotificationChanged(bool value) => IsDirty = true;

    public bool MissingFilesRootDirNotificationIsDefault => MissingFilesRootDirNotification == DefaultConfigs.Default.MissingFilesRootDirNotification;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocationLinkIsDefault))]
    private string locationLink = string.Empty;

    partial void OnLocationLinkChanged(string value) => IsDirty = true;

    public bool LocationLinkIsDefault => LocationLink == DefaultConfigs.Default.LocationLink;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileToLocationMaxDistanceIsDefault))]
    private int fileToLocationMaxDistance;

    partial void OnFileToLocationMaxDistanceChanged(int value) => IsDirty = true;

    public bool FileToLocationMaxDistanceIsDefault => FileToLocationMaxDistance == DefaultConfigs.Default.FileToLocationMaxDistance;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowModeIsDefault))]
    private WindowMode windowMode;

    partial void OnWindowModeChanged(WindowMode value) => IsDirty = true;

    public bool WindowModeIsDefault => WindowMode == DefaultConfigs.Default.WindowMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLanguageIsDefault))]
    private CultureInfo? selectedLanguage;

    partial void OnSelectedLanguageChanged(CultureInfo? value) => IsDirty = true;

    public bool SelectedLanguageIsDefault => SelectedLanguage?.Name == DefaultConfigs.Default.Language;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImageMemoryCacheCountIsDefault))]
    private int imageMemoryCacheCount;

    partial void OnImageMemoryCacheCountChanged(int value) => IsDirty = true;

    public bool ImageMemoryCacheCountIsDefault => ImageMemoryCacheCount == DefaultConfigs.Default.ImageMemoryCacheCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NumImagesToPreloadIsDefault))]
    private int numImagesToPreload;

    partial void OnNumImagesToPreloadChanged(int value) => IsDirty = true;

    public bool NumImagesToPreloadIsDefault => NumImagesToPreload == DefaultConfigs.Default.NumImagesToPreload;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayTextSizeIsDefault))]
    private int overlayTextSize;

    partial void OnOverlayTextSizeChanged(int value) => IsDirty = true;

    public bool OverlayTextSizeIsDefault => OverlayTextSize == DefaultConfigs.Default.OverlayTextSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayTextSizeLargeIsDefault))]
    private int overlayTextSizeLarge;

    partial void OnOverlayTextSizeLargeChanged(int value) => IsDirty = true;

    public bool OverlayTextSizeLargeIsDefault => OverlayTextSizeLarge == DefaultConfigs.Default.OverlayTextSizeLarge;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShortItemNameMaxLengthIsDefault))]
    private int shortItemNameMaxLength;

    partial void OnShortItemNameMaxLengthChanged(int value) => IsDirty = true;

    public bool ShortItemNameMaxLengthIsDefault => ShortItemNameMaxLength == DefaultConfigs.Default.ShortItemNameMaxLength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeIsDefault))]
    private Theme theme;

    partial void OnThemeChanged(Theme value)
    {
        IsDirty = true;
        Messenger.Send(new SetTheme(value));
    }

    public bool ThemeIsDefault => Theme == DefaultConfigs.Default.Theme;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoadExifOrientationFromFileWhenMissingInDatabaseIsDefault))]
    private bool loadExifOrientationFromFileWhenMissingInDatabase;

    partial void OnLoadExifOrientationFromFileWhenMissingInDatabaseChanged(bool value) => IsDirty = true;

    public bool LoadExifOrientationFromFileWhenMissingInDatabaseIsDefault => LoadExifOrientationFromFileWhenMissingInDatabase == DefaultConfigs.Default.LoadExifOrientationFromFileWhenMissingInDatabase;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialSearchFilterTypeIsDefault))]
    private FilterType initialSearchFilterType;

    partial void OnInitialSearchFilterTypeChanged(FilterType value) => IsDirty = true;

    public bool InitialSearchFilterTypeIsDefault => InitialSearchFilterType == DefaultConfigs.Default.InitialSearchFilterType;

    [RelayCommand]
    private void SetDefaultSlideshowDelay() => SlideshowDelay = DefaultConfigs.Default.SlideshowDelay;

    [RelayCommand]
    private void SetDefaultBackupReminder() => BackupReminder = DefaultConfigs.Default.BackupReminder;

    [RelayCommand]
    private void SetDefaultSearchHistorySize() => SearchHistorySize = DefaultConfigs.Default.SearchHistorySize;

    [RelayCommand]
    private void SetDefaultDefaultSortMethod() => DefaultSortMethod = DefaultConfigs.Default.DefaultSortMethod;

    [RelayCommand]
    private void SetDefaultKeepSelectionAfterSort() => KeepSelectionAfterSort = DefaultConfigs.Default.KeepSelectionAfterSort;

    [RelayCommand]
    private void SetDefaultBlacklistedFilePathPatterns() => BlacklistedFilePathPatterns = DefaultConfigs.Default.BlacklistedFilePathPatterns;

    [RelayCommand]
    private void SetDefaultWhitelistedFilePathPatterns() => WhitelistedFilePathPatterns = DefaultConfigs.Default.WhitelistedFilePathPatterns;

    [RelayCommand]
    private void SetDefaultIncludeHiddenDirectories() => IncludeHiddenDirectories = DefaultConfigs.Default.IncludeHiddenDirectories;

    [RelayCommand]
    private void SetDefaultReadOnly() => ReadOnly = DefaultConfigs.Default.ReadOnly;

    [RelayCommand]
    private void SetDefaultImageMemoryCacheCount() => ImageMemoryCacheCount = DefaultConfigs.Default.ImageMemoryCacheCount;

    [RelayCommand]
    private void SetDefaultNumImagesToPreload() => NumImagesToPreload = DefaultConfigs.Default.NumImagesToPreload;

    [RelayCommand]
    private void SetDefaultBirthdayReminder() => BirthdayReminder = DefaultConfigs.Default.BirthdayReminder;

    [RelayCommand]
    private void SetDefaultBirthdayReminderForDeceased() => BirthdayReminderForDeceased = DefaultConfigs.Default.BirthdayReminderForDeceased;

    [RelayCommand]
    private void SetDefaultRipReminder() => RipReminder = DefaultConfigs.Default.RipReminder;

    [RelayCommand]
    private void SetDefaultMissingFilesRootDirNotification() => MissingFilesRootDirNotification = DefaultConfigs.Default.MissingFilesRootDirNotification;

    [RelayCommand]
    private void SetDefaultLocationLink() => LocationLink = DefaultConfigs.Default.LocationLink;

    [RelayCommand]
    private void SetDefaultFileToLocationMaxDistance() => FileToLocationMaxDistance = DefaultConfigs.Default.FileToLocationMaxDistance;

    [RelayCommand]
    private void SetDefaultWindowMode() => WindowMode = DefaultConfigs.Default.WindowMode;

    [RelayCommand]
    private void SetDefaultOverlayTextSize() => OverlayTextSize = DefaultConfigs.Default.OverlayTextSize;

    [RelayCommand]
    private void SetDefaultOverlayTextSizeLarge() => OverlayTextSizeLarge = DefaultConfigs.Default.OverlayTextSizeLarge;

    [RelayCommand]
    private void SetDefaultShortItemNameMaxLength() => ShortItemNameMaxLength = DefaultConfigs.Default.ShortItemNameMaxLength;

    [RelayCommand]
    private void SetDefaultLanguage() => SelectedLanguage = Languages.FirstOrDefault(x => x.Name == DefaultConfigs.Default.Language);

    [RelayCommand]
    private void SetDefaultTheme() => Theme = DefaultConfigs.Default.Theme;

    [RelayCommand]
    private void SetDefaultLoadExifOrientationFromFileWhenMissingInDatabase() => LoadExifOrientationFromFileWhenMissingInDatabase = DefaultConfigs.Default.LoadExifOrientationFromFileWhenMissingInDatabase;

    [RelayCommand]
    private void SetDefaultInitialSearchFilterType() => InitialSearchFilterType = DefaultConfigs.Default.InitialSearchFilterType;

    private readonly IConfigProvider configProvider;
    private readonly IConfigUpdater configUpdater;
    private readonly IDialogs dialogs;
    private readonly IFileSystem fileSystem;

    public SettingsViewModel(IConfigProvider configProvider, IConfigUpdater configUpdater, IDialogs dialogs, IFileSystem fileSystem)
    {
        this.configProvider = configProvider;
        this.configUpdater = configUpdater;
        this.dialogs = dialogs;
        this.fileSystem = fileSystem;

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
        LoadExifOrientationFromFileWhenMissingInDatabase = config.LoadExifOrientationFromFileWhenMissingInDatabase;

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
            Theme,
            LoadExifOrientationFromFileWhenMissingInDatabase,
            InitialSearchFilterType);

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
