﻿using FakeItEasy;
using FileDBAvalonia;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;
using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;

namespace FileDBAvaloniaTests.ViewModels;

[TestClass]
public class NotificationsViewModelTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Config config;
    private IConfigProvider fakeConfigRepo;
    private IDatabaseAccessProvider fakeDbAccessRepo;
    private IDatabaseAccess fakeDbAccess;
    private INotifierFactory fakeNotifierFactory;
    private INotificationHandling fakeNotificationHandling;
    private INotificationsRepository fakeNotificationsRepo;

    private NotificationsViewModel viewModel;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        config = new ConfigBuilder().Build();

        fakeConfigRepo = A.Fake<IConfigProvider>();
        fakeDbAccessRepo = A.Fake<IDatabaseAccessProvider>();
        fakeDbAccess = A.Fake<IDatabaseAccess>();
        fakeNotifierFactory = A.Fake<INotifierFactory>();
        fakeNotificationHandling = A.Fake<INotificationHandling>();
        fakeNotificationsRepo = A.Fake<INotificationsRepository>();

        A.CallTo(() => fakeConfigRepo.Config).Returns(config);
        A.CallTo(() => fakeDbAccessRepo.DbAccess).Returns(fakeDbAccess);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<IDatabaseAccess>._)).Returns([]);
    }

    [TestCleanup]
    public void Cleanup()
    {
        viewModel.Close();
    }

    [TestMethod]
    public void Constructor_NoNotifications()
    {
        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

        Assert.AreEqual(0, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void Constructor_SomeNotifications()
    {
        var initialNotifications = SomeNotifications();
        A.CallTo(() => fakeNotificationsRepo.Notifications).Returns(initialNotifications);

        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

        Assert.AreEqual(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void NotificationsUpdated()
    {
        var notifications = SomeNotifications();
        A.CallTo(() => fakeNotificationsRepo.Notifications).Returns(notifications);

        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

        notifications.AddRange(SomeNotifications());
        Messenger.Send<NotificationsUpdated>();

        Assert.AreEqual(notifications.Count, viewModel.Notifications.Count);
    }

    private static List<Notification> SomeNotifications()
    {
        return
        [
            new(NotificationType.Error, $"Error text {Guid.NewGuid()}", DateTime.Now),
            new(NotificationType.Warning, $"Warning text {Guid.NewGuid()}", DateTime.Now),
            new(NotificationType.Info, $"Info text {Guid.NewGuid()}", DateTime.Now),
        ];
    }
}