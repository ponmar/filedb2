using FakeItEasy;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModel;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FileDBTests.ViewModel;

[TestClass]
public class NotificationsViewModelTests
{
    private IDbAccess fakeDbAccess;
    private IFilesystemAccess fakeFilsystemAccess;
    private INotifierFactory fakeNotifierFactory;

    private Model model;
    private NotificationsViewModel viewModel;

    [TestInitialize]
    public void Initialize()
    {
        model = Model.Instance;

        fakeDbAccess = A.Fake<IDbAccess>();
        fakeFilsystemAccess = A.Fake<IFilesystemAccess>();
        fakeNotifierFactory = A.Fake<INotifierFactory>();

        model.InitConfig(DefaultConfigs.Default, fakeDbAccess, fakeFilsystemAccess, fakeNotifierFactory);

        A.CallTo(() => model.NotifierFactory.GetContinousNotifiers()).Returns(new List<INotifier>());
        A.CallTo(() => model.NotifierFactory.GetStartupNotifiers()).Returns(new List<INotifier>());
    }

    [TestCleanup]
    public void Cleanup()
    {
        viewModel.Close();
        model.ClearNotifications();
    }

    [TestMethod]
    public void Constructor_NoNotifications()
    {
        viewModel = new NotificationsViewModel();

        Assert.AreEqual(0, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void Constructor_SomeNotifications()
    {
        var initialNotifications = SomeNotifications();
        initialNotifications.ForEach(model.AddNotification);

        viewModel = new NotificationsViewModel();

        Assert.AreEqual(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [TestMethod]
    public void NotificationsUpdated()
    {
        var initialNotifications = SomeNotifications();
        initialNotifications.ForEach(model.AddNotification);

        viewModel = new NotificationsViewModel();
        
        var newNotifications = SomeNotifications();
        newNotifications.ForEach(model.AddNotification);

        Assert.AreEqual(initialNotifications.Count + newNotifications.Count, viewModel.Notifications.Count);
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
