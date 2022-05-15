﻿using System.IO;

namespace FileDB.Configuration
{
    public enum SortMethod { Date, DateDesc, Path, PathDesc }

    public enum WindowMode { Normal, Maximized, Fullscreen }

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
        WindowMode WindowMode,
        int CastHttpServerPort);

    public static class DefaultConfigs
    {
        public const string CacheSubdir = "Cache";

        public static Config Default =>
            new(Name: "Default",
                Database: "filedb.db",
                FilesRootDirectory: "files",
                FileToLocationMaxDistance: 300,
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
                CastHttpServerPort: -1);

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
                BackupReminder: false,
                Default.BirthdayReminder,
                Default.BirthdayReminderForDeceased,
                Default.RipReminder,
                Default.MissingFilesRootDirNotification,
                Default.LocationLink,
                Default.WindowMode,
                Default.CastHttpServerPort);
    }
}
