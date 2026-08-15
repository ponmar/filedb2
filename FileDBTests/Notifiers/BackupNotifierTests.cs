using FakeItEasy;
using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Notifiers;
using FileDB.Services;
using Xunit;

namespace FileDBTests.Notifiers;

public class BackupNotifierTests
{
    private const string DatabasePath = "/files/filedb.db";

    [Fact]
    public void Run_WhenNoBackupsExist_ReturnsMissingNotification()
    {
        var fileBackup = A.Fake<IFileBackup>();
        A.CallTo(() => fileBackup.ListAvailableBackupFiles(DatabasePath)).Returns([]);

        var notifier = new BackupNotifier(fileBackup, DatabasePath, afterDays: 30);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        Assert.IsType<DatabaseBackupMissingNotification>(notification);
    }

    [Fact]
    public void Run_WhenLatestBackupIsTooOld_ReturnsWarning()
    {
        var fileBackup = A.Fake<IFileBackup>();
        var oldBackup = new BackupFile("/files/filedb_backup_2026-01-01T000000.db", DateTime.Now.AddDays(-31));
        A.CallTo(() => fileBackup.ListAvailableBackupFiles(DatabasePath)).Returns([oldBackup]);

        var notifier = new BackupNotifier(fileBackup, DatabasePath, afterDays: 30);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var backup = Assert.IsType<DatabaseBackupTooLongTimeAgoNotification>(notification);
        Assert.Equal(string.Format(Strings.BackupNotifierLongTimeSinceBackup, 31), backup.Message);
    }

    [Fact]
    public void Run_WhenLatestBackupIsRecent_ReturnsNoNotifications()
    {
        var fileBackup = A.Fake<IFileBackup>();
        var recentBackup = new BackupFile("/files/filedb_backup_2026-08-13T000000.db", DateTime.Now.AddDays(-1));
        A.CallTo(() => fileBackup.ListAvailableBackupFiles(DatabasePath)).Returns([recentBackup]);

        var notifier = new BackupNotifier(fileBackup, DatabasePath, afterDays: 30);

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}
