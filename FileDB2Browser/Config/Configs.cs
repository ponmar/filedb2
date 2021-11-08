using System;
using System.Collections.Generic;

namespace FileDB2Browser.Config
{
    public record Config(
        string Database,
        string FilesRootDirectory,
        List<string> BlacklistedFilePathPatterns,
        List<string> WhitelistedFilePathPatterns,
        bool IncludeHiddenDirectories,
        TimeSpan SlideshowDelay,
        int SearchHistorySize,
        bool ReadOnly);

    public record History(DateTime LastFilesBackupAt);

    public static class BrowserConfigFactory
    {
        public static Config GetDefault()
        {
            return new Config(
                "filedb2.db",
                "files",
                new() { "Thumbs.db", "filedb.db", "unsorted", "TN_" },
                new() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" },
                false,
                TimeSpan.FromSeconds(3),
                4,
                false);
        }
    }

    public static class HistoryFactory
    {
        public static History GetDefault()
        {
            return new History(DateTime.Now);
        }
    }
}
