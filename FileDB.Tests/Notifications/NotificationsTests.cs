using FakeItEasy;
using FileDB.Configuration;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Notifiers;
using Xunit;

namespace FileDB.Tests.Notifications;

// ── Notification model tests ────────────────────────────────────────────────

public class CollectionDemoUsedNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new CollectionDemoUsedNotification().Severity);
    [Fact] public void Message_IsNonEmpty() => Assert.NotEmpty(new CollectionDemoUsedNotification().Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new CollectionDemoUsedNotification().DateTime > DateTime.MinValue);
}

public class CollectionGetStartedNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new CollectionGetStartedNotification().Severity);
    [Fact] public void Message_IsNonEmpty() => Assert.NotEmpty(new CollectionGetStartedNotification().Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new CollectionGetStartedNotification().DateTime > DateTime.MinValue);
}

public class CollectionNoWritePermissionNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new CollectionNoWritePermissionNotification().Severity);
    [Fact] public void Message_IsNonEmpty() => Assert.NotEmpty(new CollectionNoWritePermissionNotification().Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new CollectionNoWritePermissionNotification().DateTime > DateTime.MinValue);
}

public class DatabaseBackupMissingNotificationTests
{
    [Fact] public void Severity_IsWarning() => Assert.Equal(NotificationSeverity.Warning, new DatabaseBackupMissingNotification().Severity);
    [Fact] public void Message_IsNonEmpty() => Assert.NotEmpty(new DatabaseBackupMissingNotification().Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new DatabaseBackupMissingNotification().DateTime > DateTime.MinValue);
}

public class SettingsUnsavedNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new SettingsUnsavedNotification().Severity);
    [Fact] public void Message_IsNonEmpty() => Assert.NotEmpty(new SettingsUnsavedNotification().Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new SettingsUnsavedNotification().DateTime > DateTime.MinValue);
}

public class DatabaseBackupTooLongTimeAgoNotificationTests
{
    [Fact] public void Severity_IsWarning() => Assert.Equal(NotificationSeverity.Warning, new DatabaseBackupTooLongTimeAgoNotification(30).Severity);
    [Fact] public void Message_ContainsDaysAge() => Assert.Contains("30", new DatabaseBackupTooLongTimeAgoNotification(30).Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new DatabaseBackupTooLongTimeAgoNotification(10).DateTime > DateTime.MinValue);
}

public class DatabaseMigrationErrorNotificationTests
{
    [Fact] public void Severity_IsError() => Assert.Equal(NotificationSeverity.Error, new DatabaseMigrationErrorNotification(1, 2, "oops").Severity);
    [Fact] public void Message_ContainsVersionsAndError()
    {
        var msg = new DatabaseMigrationErrorNotification(3, 7, "fail").Message;
        Assert.Contains("3", msg);
        Assert.Contains("7", msg);
        Assert.Contains("fail", msg);
    }
    [Fact] public void DateTime_IsSet() => Assert.True(new DatabaseMigrationErrorNotification(1, 2, "e").DateTime > DateTime.MinValue);
}

public class DatabaseMigrationNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new DatabaseMigrationNotification(1, 2).Severity);
    [Fact] public void Message_ContainsVersions()
    {
        var msg = new DatabaseMigrationNotification(4, 9).Message;
        Assert.Contains("4", msg);
        Assert.Contains("9", msg);
    }
    [Fact] public void DateTime_IsSet() => Assert.True(new DatabaseMigrationNotification(1, 2).DateTime > DateTime.MinValue);
}

public class DatabaseMissingNotificationTests
{
    [Fact] public void Severity_IsError() => Assert.Equal(NotificationSeverity.Error, new DatabaseMissingNotification("mydb.db").Severity);
    [Fact] public void Message_ContainsDatabasePath() => Assert.Contains("mydb.db", new DatabaseMissingNotification("mydb.db").Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new DatabaseMissingNotification("x").DateTime > DateTime.MinValue);
}

public class PersonBirthdayNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new PersonBirthdayNotification("Alice").Severity);
    [Fact] public void Message_ContainsPersonName() => Assert.Contains("Alice", new PersonBirthdayNotification("Alice").Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new PersonBirthdayNotification("Alice").DateTime > DateTime.MinValue);
}

public class PersonBirthdayForDeceasedNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new PersonBirthdayForDeceasedNotification("Bob").Severity);
    [Fact] public void Message_ContainsPersonName() => Assert.Contains("Bob", new PersonBirthdayForDeceasedNotification("Bob").Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new PersonBirthdayForDeceasedNotification("Bob").DateTime > DateTime.MinValue);
}

public class PersonRestInPeaceNotificationTests
{
    [Fact] public void Severity_IsInfo() => Assert.Equal(NotificationSeverity.Info, new PersonRestInPeaceNotification("Charlie").Severity);
    [Fact] public void Message_ContainsPersonName() => Assert.Contains("Charlie", new PersonRestInPeaceNotification("Charlie").Message);
    [Fact] public void DateTime_IsSet() => Assert.True(new PersonRestInPeaceNotification("Charlie").DateTime > DateTime.MinValue);
}

// ── NotificationRepository tests ─────────────────────────────────────────────

public class NotificationRepositoryTests
{
    private readonly INotifierFactory fakeNotifierFactory = A.Fake<INotifierFactory>();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();

    private NotificationRepository CreateSut()
    {
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());
        return new NotificationRepository(fakeNotifierFactory, fakeConfigProvider, fakeDbAccessProvider);
    }

    // --- Notifications property ---

    [Fact]
    public void Notifications_InitiallyEmpty_WhenNoNotifiersReturn()
    {
        var sut = CreateSut();
        Assert.Empty(sut.Notifications);
    }

    // --- AddNotification ---

    [Fact]
    public void AddNotification_AddsToCollection()
    {
        var sut = CreateSut();
        var n = A.Fake<INotification>();
        A.CallTo(() => n.Message).Returns("msg1");

        sut.AddNotification(n);

        Assert.Single(sut.Notifications);
    }

    [Fact]
    public void AddNotification_DuplicateMessage_ReplacesExisting()
    {
        var sut = CreateSut();
        var n1 = A.Fake<INotification>();
        var n2 = A.Fake<INotification>();
        A.CallTo(() => n1.Message).Returns("same");
        A.CallTo(() => n2.Message).Returns("same");

        sut.AddNotification(n1);
        sut.AddNotification(n2);

        Assert.Single(sut.Notifications);
        Assert.Same(n2, sut.Notifications.Single());
    }

    [Fact]
    public void AddNotification_DifferentMessages_BothKept()
    {
        var sut = CreateSut();
        var n1 = A.Fake<INotification>();
        var n2 = A.Fake<INotification>();
        A.CallTo(() => n1.Message).Returns("msg1");
        A.CallTo(() => n2.Message).Returns("msg2");

        sut.AddNotification(n1);
        sut.AddNotification(n2);

        Assert.Equal(2, sut.Notifications.Count());
    }

    [Fact]
    public void AddNotification_SendsNotificationsUpdatedMessage()
    {
        var sut = CreateSut();
        var recorder = new SingleEventRecorder<NotificationsUpdated>();
        var n = A.Fake<INotification>();
        A.CallTo(() => n.Message).Returns("m");

        sut.AddNotification(n);

        recorder.AssertEventRecorded();
    }

    // --- DismissNotifications<T> ---

    [Fact]
    public void DismissNotificationsByType_RemovesMatchingType()
    {
        var sut = CreateSut();
        sut.AddNotification(new CollectionDemoUsedNotification());
        sut.AddNotification(new SettingsUnsavedNotification());

        sut.DismissNotifications<CollectionDemoUsedNotification>();

        Assert.Single(sut.Notifications);
        Assert.IsType<SettingsUnsavedNotification>(sut.Notifications.Single());
    }

    [Fact]
    public void DismissNotificationsByType_SendsMessageWhenRemoved()
    {
        var sut = CreateSut();
        sut.AddNotification(new CollectionDemoUsedNotification());
        var recorder = new SingleEventRecorder<NotificationsUpdated>();

        sut.DismissNotifications<CollectionDemoUsedNotification>();

        recorder.AssertEventRecorded();
    }

    [Fact]
    public void DismissNotificationsByType_DoesNotSendMessageWhenNothingRemoved()
    {
        var sut = CreateSut();
        var recorder = new SingleEventRecorder<NotificationsUpdated>();

        sut.DismissNotifications<CollectionDemoUsedNotification>();

        recorder.AssertNoEventsRecorded();
    }

    // --- DismissNotifications() ---

    [Fact]
    public void DismissAllNotifications_ClearsAll()
    {
        var sut = CreateSut();
        var n = A.Fake<INotification>();
        A.CallTo(() => n.Message).Returns("m");
        sut.AddNotification(n);

        sut.DismissNotifications();

        Assert.Empty(sut.Notifications);
    }

    [Fact]
    public void DismissAllNotifications_SendsMessageWhenNotEmpty()
    {
        var sut = CreateSut();
        var n = A.Fake<INotification>();
        A.CallTo(() => n.Message).Returns("m");
        sut.AddNotification(n);
        var recorder = new SingleEventRecorder<NotificationsUpdated>();

        sut.DismissNotifications();

        recorder.AssertEventRecorded();
    }

    [Fact]
    public void DismissAllNotifications_DoesNotSendMessageWhenAlreadyEmpty()
    {
        var sut = CreateSut();
        var recorder = new SingleEventRecorder<NotificationsUpdated>();

        sut.DismissNotifications();

        recorder.AssertNoEventsRecorded();
    }

    // --- Notifiers run on startup ---

    [Fact]
    public void Constructor_RunsStartupNotifiers()
    {
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());

        _ = new NotificationRepository(fakeNotifierFactory, fakeConfigProvider, fakeDbAccessProvider);

        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Constructor_RunsContinuousNotifiers()
    {
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());

        _ = new NotificationRepository(fakeNotifierFactory, fakeConfigProvider, fakeDbAccessProvider);

        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Constructor_NotifierReturnsNotification_AddedToRepository()
    {
        var notification = new CollectionDemoUsedNotification();
        var notifier = A.Fake<INotifier>();
        A.CallTo(() => notifier.Run()).Returns([notification]);
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([notifier]);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeConfigProvider.Config).Returns(new ConfigBuilder().Build());

        var sut = new NotificationRepository(fakeNotifierFactory, fakeConfigProvider, fakeDbAccessProvider);

        Assert.Single(sut.Notifications);
    }

    // --- ConfigUpdated message re-runs notifiers ---

    [Fact]
    public void ConfigUpdated_RerunsAllNotifiers()
    {
        var sut = CreateSut();
        Fake.ClearRecordedCalls(fakeNotifierFactory);

        Messenger.Send<ConfigUpdated>();

        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustHaveHappenedOnceExactly();
    }

    // --- PersonsUpdated message re-runs continuous notifiers ---

    [Fact]
    public void PersonsUpdated_RerunsContinuousNotifiers()
    {
        var sut = CreateSut();
        Fake.ClearRecordedCalls(fakeNotifierFactory);

        Messenger.Send<PersonsUpdated>();

        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<FileDBInterface.DatabaseAccess.IDatabaseAccess>._))
            .MustNotHaveHappened();
    }
}
