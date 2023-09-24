using FileDB.Model;
using FileDBInterface.DbAccess;
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
    private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

    private readonly IFileSystem fileSystem;
    private readonly IConfigRepository configRepository;

    public DatabaseBackup(IFileSystem fileSystem, IConfigRepository configRepository)
    {
        this.fileSystem = fileSystem;
        this.configRepository = configRepository;
    }

    public List<BackupFile> ListAvailableBackupFiles()
    {
        var backupFiles = new List<BackupFile>();

        foreach (var filePath in fileSystem.Directory.GetFiles(configRepository.FilePaths.FilesRootDir, "backup_*.db"))
        {
            var filenameParts = filePath.Split("_");
            if (filenameParts.Length >= 2)
            {
                var timestampString = filenameParts[^1].Replace(".db", "");
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
        var db = configRepository.FilePaths.DatabasePath;
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
