using FakeItEasy;
using FileDBAvalonia;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;
using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using Xunit;

namespace FileDBAvaloniaTests.ViewModels;

[Collection("Sequential")]
public class NotificationsViewModelTests : IDisposable
{
    private Config config;
    private IConfigProvider fakeConfigRepo;
    private IDatabaseAccessProvider fakeDbAccessRepo;
    private IDatabaseAccess fakeDbAccess;
    private INotifierFactory fakeNotifierFactory;
    private INotificationHandling fakeNotificationHandling;
    private INotificationsRepository fakeNotificationsRepo;

    private NotificationsViewModel? viewModel;

    public NotificationsViewModelTests()
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
