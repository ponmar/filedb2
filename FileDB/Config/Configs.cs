using System;
using System.Collections.Generic;
using System.IO;

namespace FileDB.Config
{
    public record Config(
        string Name,
        string Database,
        string FilesRootDirectory,
        double FileToLocationMaxDistance,
        List<string> BlacklistedFilePathPatterns,
        List<string> WhitelistedFilePathPatterns,
        bool IncludeHiddenDirectories,
        TimeSpan SlideshowDelay,
        int SearchHistorySize,
        bool ReadOnly,
        int StartupBackupReminderAfterDays,
        bool BirthdayReminder,
        bool RipReminder,
        string LocationLink);

    public static class BrowserConfigFactory
    {
        public static Config CreateDefaultConfig()
        {
            return new Config(
                "Default",
                "filedb.db",
                "files",
                300,
                new() { "Thumbs.db", "filedb.db", "unsorted", "TN_" },
                new() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" },
                false,
                TimeSpan.FromSeconds(3),
                4,
                false,
                30,
                true,
                true,
                "https://www.google.com/maps?q=loc:LAT,LON");
        }

        public static Config CreateDemoConfig()
        {
            var defaultConfig = CreateDefaultConfig();
            return new Config(
                "Demo",
                Path.Combine(Directory.GetCurrentDirectory(), @"demo\filedb.db"),
                Path.Combine(Directory.GetCurrentDirectory(), @"demo\files"),
                defaultConfig.FileToLocationMaxDistance,
                defaultConfig.BlacklistedFilePathPatterns,
                defaultConfig.WhitelistedFilePathPatterns,
                defaultConfig.IncludeHiddenDirectories,
                defaultConfig.SlideshowDelay,
                defaultConfig.SearchHistorySize,
                defaultConfig.ReadOnly,
                0,
                defaultConfig.BirthdayReminder,
                defaultConfig.RipReminder,
                defaultConfig.LocationLink);
        }
    }
}
