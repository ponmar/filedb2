using FakeItEasy;
using FileDB;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModel;
using FileDBInterface.DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FileDBTests.ViewModel;

[TestClass]
public class NotificationsViewModelTests
{
    private Config config;
    private IConfigRepository fakeConfigRepo;
    private IDbAccessRepository fakeDbAccessRepo;
    private IDbAccess fakeDbAccess;
    private INotifierFactory fakeNotifierFactory;
    private INotificationHandling fakeNotificationHandling;
    private INotificationsRepository fakeNotificationsRepo;

    private NotificationsViewModel viewModel;

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        config = new ConfigBuilder().Build();

        fakeConfigRepo = A.Fake<IConfigRepository>();
        fakeDbAccessRepo = A.Fake<IDbAccessRepository>();
        fakeDbAccess = A.Fake<IDbAccess>();
        fakeNotifierFactory = A.Fake<INotifierFactory>();
        fakeNotificationHandling = A.Fake<INotificationHandling>();
        fakeNotificationsRepo = A.Fake<INotificationsRepository>();

        A.CallTo(() => fakeConfigRepo.Config).Returns(config);
        A.CallTo(() => fakeDbAccessRepo.DbAccess).Returns(fakeDbAccess);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<IDbAccess>._)).Returns(new List<INotifier>());
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<IDbAccess>._)).Returns(new List<INotifier>());
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
        Events.Send<NotificationsUpdated>();

        Assert.AreEqual(notifications.Count, viewModel.Notifications.Count);
    }

    private static List<Notification> SomeNotifications()
    {
        return new()
        {
            new(NotificationType.Error, $"Error text {Guid.NewGuid()}", DateTime.Now),
            new(NotificationType.Warning, $"Warning text {Guid.NewGuid()}", DateTime.Now),
            new(NotificationType.Info, $"Info text {Guid.NewGuid()}", DateTime.Now),
        };
    }
}
