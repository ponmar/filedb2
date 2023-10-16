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

public class FileBackup
{
    private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

    private readonly IFileSystem fileSystem;
    private readonly string filePath;

    public FileBackup(IFileSystem fileSystem, string filePath)
    {
        this.fileSystem = fileSystem;
        this.filePath = filePath;
    }

    public List<BackupFile> ListAvailableBackupFiles()
    {
        var backupFiles = new List<BackupFile>();

        var backupDir = Path.GetDirectoryName(filePath)!;
        var fileExtension = Path.GetExtension(filePath);
        var backupFilesPattern = Path.GetFileNameWithoutExtension(filePath) + "_backup_*" + fileExtension;

        foreach (var filePath in fileSystem.Directory.GetFiles(backupDir, backupFilesPattern))
        {
            var filenameParts = filePath.Split("_");
            if (filenameParts.Length >= 2)
            {
                var timestampString = filenameParts[^1].Replace(fileExtension, "");
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
        if (!fileSystem.File.Exists(filePath))
        {
            throw new IOException($"Database to backup does not exist: {filePath}");
        }

        var timestamp = DateTime.Now.ToString(BackupFileTimestampFormat);
        var directoryPath = Path.GetDirectoryName(filePath);
        var backupFilename = $"{Path.GetFileNameWithoutExtension(filePath)}_backup_{timestamp}{Path.GetExtension(filePath)}";
        var backupFilePath = Path.Combine(directoryPath!, backupFilename);

        if (fileSystem.File.Exists(backupFilePath))
        {
            throw new IOException($"Backup file already exists: {backupFilePath}");
        }

        fileSystem.File.Copy(filePath, backupFilePath);
    }
}
