using FakeItEasy;
using FileDB;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModels;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBTests.ViewModels;

public class NotificationsViewModelTests : IDisposable
{
    private readonly Config config = new ConfigBuilder().Build();
    private readonly IConfigProvider fakeConfigRepo = A.Fake<IConfigProvider>();
    private readonly IDatabaseAccessProvider fakeDbAccessRepo = A.Fake<IDatabaseAccessProvider>();
    private readonly IDatabaseAccess fakeDbAccess = A.Fake<IDatabaseAccess>();
    private readonly INotifierFactory fakeNotifierFactory = A.Fake<INotifierFactory>();
    private readonly INotificationHandling fakeNotificationHandling = A.Fake<INotificationHandling>();
    private readonly INotificationsRepository fakeNotificationsRepo = A.Fake<INotificationsRepository>();

    private NotificationsViewModel? viewModel;

    public NotificationsViewModelTests()
    {
        A.CallTo(() => fakeConfigRepo.Config).Returns(config);
        A.CallTo(() => fakeDbAccessRepo.DbAccess).Returns(fakeDbAccess);
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
        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

        Assert.Empty(viewModel.Notifications);
    }

    [Fact]
    public void Constructor_SomeNotifications()
    {
        var initialNotifications = SomeNotifications();
        A.CallTo(() => fakeNotificationsRepo.Notifications).Returns(initialNotifications);

        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

        Assert.Equal(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [Fact]
    public void NotificationsUpdated()
    {
        var notifications = SomeNotifications();
        A.CallTo(() => fakeNotificationsRepo.Notifications).Returns(notifications);

        viewModel = new NotificationsViewModel(fakeConfigRepo, fakeDbAccessRepo, fakeNotifierFactory, fakeNotificationHandling, fakeNotificationsRepo);

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
