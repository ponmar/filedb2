using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FileDB.Services;
using Xunit;

namespace FileDBTests.Services;

public class FileBackupTests
{
    [Fact]
    public void ListAvailableBackupFiles_ReturnsParsedBackupsOnly()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-services-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "collection.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(databasePath, new MockFileData("db"));

        var backup1 = CreateBackupFilePath(databasePath, new DateTime(2026, 8, 5, 10, 20, 30));
        var backup2 = CreateBackupFilePath(databasePath, new DateTime(2026, 8, 4, 9, 8, 7));
        fileSystem.AddFile(backup1, new MockFileData(""));
        fileSystem.AddFile(backup2, new MockFileData(""));
        fileSystem.AddFile(Path.Combine(rootDirectory, "collection_backup_invalid.db"), new MockFileData(""));

        var fileBackup = new FileBackup(fileSystem, NullLoggerFactory.Instance);

        var result = fileBackup.ListAvailableBackupFiles(databasePath).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Filename == backup1 && x.Timestamp == new DateTime(2026, 8, 5, 10, 20, 30));
        Assert.Contains(result, x => x.Filename == backup2 && x.Timestamp == new DateTime(2026, 8, 4, 9, 8, 7));
    }

    [Fact]
    public void CreateBackup_CopiesDatabaseFile()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-services-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "collection.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(databasePath, new MockFileData("db"));

        var fileBackup = new FileBackup(fileSystem, NullLoggerFactory.Instance);
        fileBackup.CreateBackup(databasePath);

        var files = fileSystem.Directory.GetFiles(rootDirectory);
        var backupFile = Assert.Single(files, x => x.Contains("_backup_"));
        Assert.Equal("db", fileSystem.File.ReadAllText(backupFile));
    }

    [Fact]
    public void CreateBackup_WhenSourceFileMissing_ThrowsIOException()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-services-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "collection.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);

        var fileBackup = new FileBackup(fileSystem, NullLoggerFactory.Instance);

        Assert.Throws<IOException>(() => fileBackup.CreateBackup(databasePath));
    }

    private static string CreateBackupFilePath(string databasePath, DateTime timestamp)
    {
        var directory = Path.GetDirectoryName(databasePath)!;
        var filename = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        return Path.Combine(directory, $"{filename}_backup_{timestamp:yyyy-MM-ddTHHmmss}{extension}");
    }
}
