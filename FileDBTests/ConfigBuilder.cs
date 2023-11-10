using FileDB.Configuration;

namespace FileDBTests;

public class ConfigBuilder
{
    public int FileToLocationMaxDistance { get; init; }
    public string BlacklistedFilePathPatterns { get; init; } = string.Empty;
    public string WhitelistedFilePathPatterns { get; init; } = string.Empty;
    public bool IncludeHiddenDirectories { get; init; }
    public int SlideshowDelay { get; init; }
    public int SearchHistorySize { get; init; }
    public SortMethod DefaultSortMethod { get; init; }
    public bool KeepSelectionAfterSort { get; init; }
    public bool ReadOnly { get; init; }
    public bool BackupReminder { get; init; }
    public bool BirthdayReminder { get; init; }
    public bool BirthdayReminderForDeceased { get; init; }
    public bool RipReminder { get; init; }
    public bool MissingFilesRootDirNotification { get; init; }
    public string LocationLink { get; init; } = string.Empty;
    public WindowMode WindowMode { get; init; }
    public int ImageMemoryCacheCount { get; init; }
    public int NumImagesToPreload { get; init; }
    public int OverlayTextSize { get; init; }
    public int OverlayTextSizeLarge { get; init; }
    public int ShortItemNameMaxLength { get; init; }
    public string? Language { get; init; }

    public Config Build()
    {
        return new(
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
            Language);
    }
}
