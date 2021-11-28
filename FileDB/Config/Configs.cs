﻿using System.Collections.Generic;
using System.IO;

namespace FileDB.Config
{
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
        bool ReadOnly,
        int StartupBackupReminderAfterDays,
        bool BirthdayReminder,
        bool BirthdayReminderForDeceased,
        bool RipReminder,
        string LocationLink);

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
                false,
                30,
                true,
                true,
                true,
                "https://www.google.com/maps?q=loc:LAT,LON");

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
                Default.ReadOnly,
                0,
                Default.BirthdayReminder,
                Default.BirthdayReminderForDeceased,
                Default.RipReminder,
                Default.LocationLink);
    }
}
