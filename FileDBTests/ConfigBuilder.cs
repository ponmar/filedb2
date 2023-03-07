﻿using FileDB.Configuration;

namespace FileDBTests;

public class ConfigBuilder
{
    public string Name { get; init; }
    public string Database { get; init; }
    public string FilesRootDirectory { get; init; }
    public int FileToLocationMaxDistance { get; init; }
    public string BlacklistedFilePathPatterns { get; init; }
    public string WhitelistedFilePathPatterns { get; init; }
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
    public string LocationLink { get; init; }
    public WindowMode WindowMode { get; init; }
    public bool CacheFiles { get; init; }
    public int OverlayTextSize { get; init; }
    public int OverlayTextSizeLarge { get; init; }
    public int ShortItemNameMaxLength { get; init; }
    public string? CultureOverride { get; init; }

    public Config Build()
    {
        return new(
            Name,
            Database,
            FilesRootDirectory,
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
            CacheFiles,
            OverlayTextSize,
            OverlayTextSizeLarge,
            ShortItemNameMaxLength,
            CultureOverride);
    }
}