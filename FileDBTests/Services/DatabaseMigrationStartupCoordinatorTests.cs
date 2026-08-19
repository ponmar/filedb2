using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Services;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FileDBTests.Services;

public class DatabaseMigrationStartupCoordinatorTests
{
    private readonly IDialogs dialogs;
    private readonly IFileBackup fileBackup;
    private readonly IDatabaseAccess dbAccess;
    private readonly DatabaseMigrationStartupCoordinator sut;

    public DatabaseMigrationStartupCoordinatorTests()
    {
        dialogs = A.Fake<IDialogs>();
        fileBackup = A.Fake<IFileBackup>();
        dbAccess = A.Fake<IDatabaseAccess>();

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).Returns(Task.CompletedTask);
        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._, A<Exception>._)).Returns(Task.CompletedTask);

        sut = new DatabaseMigrationStartupCoordinator(dialogs, fileBackup, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task TryHandleMigrationAsync_NoMigrationNeeded_ReturnsTrue()
    {
        A.CallTo(() => dbAccess.NeedsMigration).Returns(false);
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: false, notifications);

        Assert.True(result);
        A.CallTo(() => fileBackup.CreateBackup(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => dbAccess.Migrate()).MustNotHaveHappened();
        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TryHandleMigrationAsync_ReadOnlyModeAndMigrationNeeded_ShowsErrorAndReturnsFalse()
    {
        A.CallTo(() => dbAccess.NeedsMigration).Returns(true);
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: true, notifications);

        Assert.False(result);
        A.CallTo(() => dialogs.ShowErrorDialogAsync(Strings.AppMigrationRequiredInReadOnlyMode)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fileBackup.CreateBackup(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => dbAccess.Migrate()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TryHandleMigrationAsync_BackupFails_ShowsErrorAndReturnsFalse()
    {
        A.CallTo(() => dbAccess.NeedsMigration).Returns(true);
        A.CallTo(() => fileBackup.CreateBackup(@"C:\db\collection.db")).Throws(new InvalidOperationException("backup failed"));
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: false, notifications);

        Assert.False(result);
        A.CallTo(() => dialogs.ShowErrorDialogAsync(
            Strings.AppUnableToCreateDatabaseBackupBeforeMigration,
            A<Exception>.That.Matches(e => e.Message == "backup failed"))).MustHaveHappenedOnceExactly();
        A.CallTo(() => dbAccess.Migrate()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TryHandleMigrationAsync_MigrationFailure_ShowsErrorAndStopsProcessingFurtherResults()
    {
        A.CallTo(() => dbAccess.NeedsMigration).Returns(true);
        A.CallTo(() => dbAccess.Migrate()).Returns([
            new DatabaseMigrationResult(0, 1),
            new DatabaseMigrationResult(1, 2, new InvalidOperationException("broken migration")),
            new DatabaseMigrationResult(2, 3)
        ]);
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: false, notifications);

        Assert.False(result);
        Assert.Equal(2, notifications.Count);
        Assert.IsType<DatabaseMigrationNotification>(notifications[0]);
        Assert.IsType<DatabaseMigrationErrorNotification>(notifications[1]);
        A.CallTo(() => dialogs.ShowErrorDialogAsync(
            string.Format(Strings.NotificationDatabaseMigrationError, 1, 2, "broken migration"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TryHandleMigrationAsync_AllMigrationsSucceed_ReturnsTrueAndAddsNotifications()
    {
        A.CallTo(() => dbAccess.NeedsMigration).Returns(true);
        A.CallTo(() => dbAccess.Migrate()).Returns([
            new DatabaseMigrationResult(0, 1),
            new DatabaseMigrationResult(1, 2)
        ]);
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: false, notifications);

        Assert.True(result);
        Assert.Equal(2, notifications.Count);
        Assert.All(notifications, x => Assert.IsType<DatabaseMigrationNotification>(x));
        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TryHandleMigrationAsync_DatabaseTooNew_ShowsErrorAndReturnsFalse()
    {
        A.CallTo(() => dbAccess.IsTooNew).Returns(true);
        var notifications = new List<INotification>();

        var result = await sut.TryHandleMigrationAsync(dbAccess, @"C:\db\collection.db", readOnly: false, notifications);

        Assert.False(result);
        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fileBackup.CreateBackup(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => dbAccess.Migrate()).MustNotHaveHappened();
        Assert.Empty(notifications);
    }
}
