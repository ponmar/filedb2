using FakeItEasy;
using FileDB;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Notifiers;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels;

public class NotificationsViewModelTests : IDisposable
{
    private readonly Config config = new ConfigBuilder().Build();
    private readonly IConfigProvider fakeConfigProvider = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider fakeDbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess fakeDbAccess = A.Fake<IDatabaseAccess>();
    private readonly INotifierFactory fakeNotifierFactory = A.Fake<INotifierFactory>();
    private readonly INotificationManagement fakeNotificationManagement = A.Fake<INotificationManagement>();
    private readonly INotificationRepository fakeNotificationRepo = A.Fake<INotificationRepository>();

    private NotificationsViewModel? viewModel;

    public NotificationsViewModelTests()
    {
        A.CallTo(() => fakeConfigProvider.Config).Returns(config);
        A.CallTo(() => fakeDbAccessProvider.DbAccess).Returns(fakeDbAccess);
        A.CallTo(() => fakeNotifierFactory.GetContinousNotifiers(A<Config>._, A<IDatabaseAccess>._)).Returns([]);
        A.CallTo(() => fakeNotifierFactory.GetStartupNotifiers(A<Config>._, A<IDatabaseAccess>._)).Returns([]);
    }

    public void Dispose()
    {
        viewModel?.Close();
    }

    [Fact]
    public void Constructor_NoNotifications()
    {
        viewModel = new NotificationsViewModel(fakeConfigProvider, fakeDbAccessProvider, fakeNotifierFactory, fakeNotificationManagement, fakeNotificationRepo);

        Assert.Empty(viewModel.Notifications);
    }

    [Fact]
    public void Constructor_SomeNotifications()
    {
        var initialNotifications = SomeNotifications();
        A.CallTo(() => fakeNotificationRepo.Notifications).Returns(initialNotifications);

        viewModel = new NotificationsViewModel(fakeConfigProvider, fakeDbAccessProvider, fakeNotifierFactory, fakeNotificationManagement, fakeNotificationRepo);

        Assert.Equal(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [Fact]
    public void NotificationsUpdated()
    {
        var notifications = SomeNotifications();
        A.CallTo(() => fakeNotificationRepo.Notifications).Returns(notifications);

        viewModel = new NotificationsViewModel(fakeConfigProvider, fakeDbAccessProvider, fakeNotifierFactory, fakeNotificationManagement, fakeNotificationRepo);

        notifications.AddRange(SomeNotifications());
        Messenger.Send<NotificationsUpdated>();

        Assert.Equal(notifications.Count, viewModel.Notifications.Count);
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
