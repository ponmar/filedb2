namespace FileDBAvalonia.Configuration;

public enum Theme { Default, Dark, Light }

public record Config(
    int FileToLocationMaxDistance,
    string BlacklistedFilePathPatterns,
    string WhitelistedFilePathPatterns,
    bool IncludeHiddenDirectories,
    int SlideshowDelay,
    int SearchHistorySize,
    SortMethod DefaultSortMethod,
    bool KeepSelectionAfterSort,
    bool ReadOnly,
    bool BackupReminder,
    bool BirthdayReminder,
    bool BirthdayReminderForDeceased,
    bool RipReminder,
    bool MissingFilesRootDirNotification,
    string LocationLink,
    WindowMode WindowMode,
    int ImageMemoryCacheCount,
    int NumImagesToPreload,
    int OverlayTextSize,
    int OverlayTextSizeLarge,
    int ShortItemNameMaxLength,
    string? Language,
    Theme Theme);

public static class DefaultConfigs
{
    public static Config Default =>
        new(FileToLocationMaxDistance: 300,
            BlacklistedFilePathPatterns: "Thumbs.db;filedb.db;unsorted;TN_",
            WhitelistedFilePathPatterns: ".jpg;.png;.bmp;.gif;.avi;.mpg;.mp4;.mkv;.mov;.pdf",
            IncludeHiddenDirectories: false,
            SlideshowDelay: 3,
            SearchHistorySize: 4,
            DefaultSortMethod: SortMethod.Date,
            KeepSelectionAfterSort: false,
            ReadOnly: false,
            BackupReminder: true,
            BirthdayReminder: true,
            BirthdayReminderForDeceased: true,
            RipReminder: true,
            MissingFilesRootDirNotification: true,
            LocationLink: "https://www.google.com/maps?q=loc:LAT,LON",
            WindowMode: WindowMode.Maximized,
            ImageMemoryCacheCount: 50,
            NumImagesToPreload: 3,
            OverlayTextSize: 14,
            OverlayTextSizeLarge: 24,
            ShortItemNameMaxLength: 15,
            Language: null,
            Theme: Theme.Default);
}
