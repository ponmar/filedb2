using CommunityToolkit.Mvvm.Messaging;
using FakeItEasy;
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
    private IDbAccess fakeDbAccess;
    private INotifierFactory fakeNotifierFactory;
    private INotificationHandling fakeNotificationHandling;

    private NotificationsViewModel viewModel;

    [TestInitialize]
    public void Initialize()
    {
        fakeDbAccess = A.Fake<IDbAccess>();
        fakeNotifierFactory = A.Fake<INotifierFactory>();
        fakeNotificationHandling = A.Fake<INotificationHandling>();

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
        var config = new ConfigBuilder().Build();
        viewModel = new NotificationsViewModel(config, fakeDbAccess, fakeNotifierFactory, fakeNotificationHandling);

        Assert.AreEqual(0, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void Constructor_SomeNotifications()
    {
        var initialNotifications = SomeNotifications();
        A.CallTo(() => fakeNotificationHandling.Notifications).Returns(initialNotifications);
        
        var config = new ConfigBuilder().Build();
        viewModel = new NotificationsViewModel(config, fakeDbAccess, fakeNotifierFactory, fakeNotificationHandling);

        Assert.AreEqual(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void NotificationsUpdated()
    {
        var notifications = SomeNotifications();
        A.CallTo(() => fakeNotificationHandling.Notifications).Returns(notifications);

        var config = new ConfigBuilder().Build();
        viewModel = new NotificationsViewModel(config, fakeDbAccess, fakeNotifierFactory, fakeNotificationHandling);
        
        notifications.AddRange(SomeNotifications());
        WeakReferenceMessenger.Default.Send(new NotificationsUpdated());

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
