using System.IO;
using FileDB.Sorters;

namespace FileDB.Config
{
    public enum WindowMode { Normal, Maximized, Fullscreen }

    public static class WindowModeExtensions
    {
        public static string GetDescription(this WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Normal:
                    return "Window";

                case WindowMode.Maximized:
                    return "Maximized window";

                case WindowMode.Fullscreen:
                    return "Fullscreen window";
            }
            return string.Empty;
        }
    }

    public record Config(
        string Name,
        string Database,
        string FilesRootDirectory,
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
        WindowMode WindowMode);

    public static class DefaultConfigs
    {
        public static Config Default =>
            new("Default",
                "filedb.db",
                "files",
                300,
                "Thumbs.db;filedb.db;unsorted;TN_",
                ".jpg;.png;.bmp;.gif;.avi;.mpg;.mp4;.mkv;.mov;.pdf",
                false,
                3,
                4,
                SortMethod.Date,
                false,
                false,
                true,
                true,
                true,
                true,
                true,
                "https://www.google.com/maps?q=loc:LAT,LON",
                WindowMode.Maximized);

        public static Config CreateDemo() =>
            new("Demo",
                Path.Combine(Directory.GetCurrentDirectory(), @"demo\filedb.db"),
                Path.Combine(Directory.GetCurrentDirectory(), @"demo\files"),
                Default.FileToLocationMaxDistance,
                Default.BlacklistedFilePathPatterns,
                Default.WhitelistedFilePathPatterns,
                Default.IncludeHiddenDirectories,
                Default.SlideshowDelay,
                Default.SearchHistorySize,
                Default.DefaultSortMethod,
                Default.KeepSelectionAfterSort,
                Default.ReadOnly,
                false,
                Default.BirthdayReminder,
                Default.BirthdayReminderForDeceased,
                Default.RipReminder,
                Default.MissingFilesRootDirNotification,
                Default.LocationLink,
                Default.WindowMode);
    }
}
