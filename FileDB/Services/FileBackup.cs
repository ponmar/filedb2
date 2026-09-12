using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace FileDB.Services;

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

public interface IFileBackup
{
    List<BackupFile> ListAvailableBackupFiles(string filePath);
    void CreateBackup(string filePath);
}

public class FileBackup : IFileBackup
{
    private const string BackupFileTimestampFormat = "yyyy-MM-ddTHHmmss";

    private readonly IFileSystem fileSystem;
    private readonly ILogger<FileBackup> logger;

    public FileBackup(IFileSystem fileSystem, ILoggerFactory loggerFactory)
    {
        this.fileSystem = fileSystem;
        this.logger = loggerFactory.CreateLogger<FileBackup>();
    }

    public List<BackupFile> ListAvailableBackupFiles(string filePath)
    {
        var backupFiles = new List<BackupFile>();

        var backupDir = Path.GetDirectoryName(filePath)!;
        var fileExtension = Path.GetExtension(filePath);
        var backupFilesPattern = Path.GetFileNameWithoutExtension(filePath) + "_backup_*" + fileExtension;

        if (fileSystem.Directory.Exists(backupDir))
        {
            foreach (var file in fileSystem.Directory.GetFiles(backupDir, backupFilesPattern))
            {
                var filenameParts = file.Split("_");
                if (filenameParts.Length >= 2)
                {
                    var timestampString = filenameParts[^1].Replace(fileExtension, "");
                    try
                    {
                        var timestamp = DateTime.ParseExact(timestampString, BackupFileTimestampFormat, CultureInfo.InvariantCulture);
                        backupFiles.Add(new BackupFile(file, timestamp));
                    }
                    catch (FormatException)
                    {
                    }
                }
            }
        }

        return backupFiles;
    }

    public void CreateBackup(string filePath)
    {
        if (!fileSystem.File.Exists(filePath))
        {
            throw new IOException($"File to backup does not exist: {filePath}");
        }

        var timestamp = DateTime.Now.ToString(BackupFileTimestampFormat, CultureInfo.InvariantCulture);
        var directoryPath = Path.GetDirectoryName(filePath);
        var backupFilename = $"{Path.GetFileNameWithoutExtension(filePath)}_backup_{timestamp}{Path.GetExtension(filePath)}";
        var backupFilePath = Path.Combine(directoryPath!, backupFilename);

        if (fileSystem.File.Exists(backupFilePath))
        {
            throw new IOException($"Backup file already exists: {backupFilePath}");
        }

        fileSystem.File.Copy(filePath, backupFilePath);
        logger.LogInformation("Database backup created: {BackupFilePath}", backupFilePath);
    }
}
