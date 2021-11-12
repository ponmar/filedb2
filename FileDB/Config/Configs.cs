using System;
using System.Collections.Generic;

namespace FileDB.Config
{
    public record Config(
        string Database,
        string FilesRootDirectory,
        List<string> BlacklistedFilePathPatterns,
        List<string> WhitelistedFilePathPatterns,
        bool IncludeHiddenDirectories,
        TimeSpan SlideshowDelay,
        int SearchHistorySize,
        bool ReadOnly,
        int StartupBackupReminderAfterDays);

    public static class BrowserConfigFactory
    {
        public static Config CreateDefaultConfig()
        {
            return new Config(
                "filedb.db",
                "files",
                new() { "Thumbs.db", "filedb.db", "unsorted", "TN_" },
                new() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" },
                false,
                TimeSpan.FromSeconds(3),
                4,
                false,
                30);
        }

        public static Config CreateDemoConfig()
        {
            var defaultConfig = CreateDefaultConfig();
            return new Config(
                @"demo\filedb.db",
                @"demo\files",
                defaultConfig.BlacklistedFilePathPatterns,
                defaultConfig.WhitelistedFilePathPatterns,
                defaultConfig.IncludeHiddenDirectories,
                defaultConfig.SlideshowDelay,
                defaultConfig.SearchHistorySize,
                defaultConfig.ReadOnly,
                0);
        }
    }
}
