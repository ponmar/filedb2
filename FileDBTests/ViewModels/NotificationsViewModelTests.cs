using FakeItEasy;
using FileDB;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.ViewModels;
using Xunit;

namespace FileDBTests.ViewModels;

public class NotificationsViewModelTests
{
    private readonly INotificationManagement fakeNotificationManagement = A.Fake<INotificationManagement>();
    private readonly INotificationRepository fakeNotificationRepo = A.Fake<INotificationRepository>();

    [Fact]
    public void Constructor_NoNotifications()
    {
        // Act
        var viewModel = new NotificationsViewModel(fakeNotificationManagement, fakeNotificationRepo);

        // Assert
        Assert.Empty(viewModel.Notifications);
    }

    [Fact]
    public void Constructor_SomeNotifications()
    {
        // Arrange
        var initialNotifications = SomeNotifications();
        A.CallTo(() => fakeNotificationRepo.Notifications).Returns(initialNotifications);

        // Act
        var viewModel = new NotificationsViewModel(fakeNotificationManagement, fakeNotificationRepo);

        // Assert
        Assert.Equal(initialNotifications.Count, viewModel.Notifications.Count);
    }

    [Fact]
    public void NotificationsUpdated()
    {
        // Arrange
        var notifications = SomeNotifications();
        A.CallTo(() => fakeNotificationRepo.Notifications).Returns(notifications);

        var viewModel = new NotificationsViewModel(fakeNotificationManagement, fakeNotificationRepo);

        notifications.AddRange(SomeNotifications());

        // Act
        Messenger.Send<NotificationsUpdated>();

        // Assert
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
