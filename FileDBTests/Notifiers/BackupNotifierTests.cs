using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Notifiers;
using FileDB.Services;
using Xunit;

namespace FileDBTests.Notifiers;

public class BackupNotifierTests
{
    [Fact]
    public void Run_WhenNoBackupsExist_ReturnsMissingNotification()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-backup-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "filedb.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(databasePath, new MockFileData(""));

        var notifier = new BackupNotifier(new FileBackup(fileSystem, databasePath), afterDays: 30);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        Assert.IsType<DatabaseBackupMissingNotification>(notification);
    }

    [Fact]
    public void Run_WhenLatestBackupIsTooOld_ReturnsWarning()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-backup-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "filedb.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(databasePath, new MockFileData(""));

        var timestamp = DateTime.Now.AddDays(-31);
        fileSystem.AddFile(NotifierTestHelpers.CreateBackupFilePath(databasePath, timestamp), new MockFileData(""));

        var notifier = new BackupNotifier(new FileBackup(fileSystem, databasePath), afterDays: 30);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var backup = Assert.IsType<DatabaseBackupTooLongTimeAgoNotification>(notification);
        Assert.Equal(string.Format(Strings.BackupNotifierLongTimeSinceBackup, 31), backup.Message);
    }

    [Fact]
    public void Run_WhenLatestBackupIsRecent_ReturnsNoNotifications()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-backup-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(rootDirectory, "filedb.db");
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(databasePath, new MockFileData(""));

        var timestamp = DateTime.Now.AddDays(-1);
        fileSystem.AddFile(NotifierTestHelpers.CreateBackupFilePath(databasePath, timestamp), new MockFileData(""));

        var notifier = new BackupNotifier(new FileBackup(fileSystem, databasePath), afterDays: 30);

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}
