﻿using FileDB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FileDB;

public class BackupFile
{
    public string Filename { get; }

    public DateTime Timestamp { get; }

    public TimeSpan Age => DateTime.Now - Timestamp;

    public BackupFile(string filename, DateTime timestamp)
    {
        Filename = filename;
        Timestamp = timestamp;
    }
}

public class DatabaseBackup
{
    public string BackupDirectory { get; }

    private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

    private readonly IConfigRepository configRepository;
    private readonly IFileSystem fileSystem;

    public DatabaseBackup(IConfigRepository configRepository, IFileSystem fileSystem)
    {
        this.configRepository = configRepository;
        this.fileSystem = fileSystem;
        BackupDirectory = Path.GetDirectoryName(configRepository.Config.Database)!;
    }

    public List<BackupFile> ListAvailableBackupFiles()
    {
        var backupFiles = new List<BackupFile>();

        foreach (var filePath in fileSystem.Directory.GetFiles(BackupDirectory, "backup_*.db"))
        {
            var filenameParts = filePath.Split("_");
            if (filenameParts.Length >= 2)
            {
                var timestampString = filenameParts[filenameParts.Length - 1].Replace(".db", "");
                try
                {
                    var timestamp = DateTime.ParseExact(timestampString, BackupFileTimestampFormat, null);
                    backupFiles.Add(new BackupFile(filePath, timestamp));
                }
                catch (FormatException)
                {
                }
            }
        }

        return backupFiles;
    }

    public void CreateBackup()
    {
        var db = configRepository.Config.Database;
        if (!fileSystem.File.Exists(db))
        {
            throw new IOException($"Database to backup does not exist: {db}");
        }

        var directoryPath = Path.GetDirectoryName(db);
        var timestamp = DateTime.Now.ToString(BackupFileTimestampFormat);
        var backupFilename = $"backup_{timestamp}.db";
        var backupFilePath = Path.Combine(directoryPath!, backupFilename);

        if (fileSystem.File.Exists(backupFilePath))
        {
            throw new IOException($"Backup file already exists: {backupFilePath}");
        }

        fileSystem.File.Copy(db, backupFilePath);
    }
}
