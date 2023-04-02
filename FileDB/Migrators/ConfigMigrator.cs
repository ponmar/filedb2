using FileDB.Configuration;

namespace FileDB.Migrators;

public class ConfigMigrator : IMigrator<Config>
{
    public Config Migrate(Config config, Config defaultValues)
    {
        return new Config(
            config.Name,
            config.Database,
            config.FilesRootDirectory,
            config.FileToLocationMaxDistance,
            config.BlacklistedFilePathPatterns,
            config.WhitelistedFilePathPatterns,
            config.IncludeHiddenDirectories,
            config.SlideshowDelay,
            config.SearchHistorySize,
            config.DefaultSortMethod,
            config.KeepSelectionAfterSort,
            config.ReadOnly,
            config.BackupReminder,
            config.BirthdayReminder,
            config.BirthdayReminderForDeceased,
            config.RipReminder,
            config.MissingFilesRootDirNotification,
            config.LocationLink,
            config.WindowMode,
            config.ImageMemoryCacheCount,
            config.NumImagesToPreload,
            config.OverlayTextSize != 0 ? config.OverlayTextSize : defaultValues.OverlayTextSize,
            config.OverlayTextSizeLarge != 0 ? config.OverlayTextSizeLarge : defaultValues.OverlayTextSizeLarge,
            config.ShortItemNameMaxLength != 0 ? config.ShortItemNameMaxLength : defaultValues.ShortItemNameMaxLength,
            config.CultureOverride);
    }
}
