using System.Collections.Generic;
using FileDB.Model;

namespace FileDB.Notifications;

public interface INotificationRepository
{
    IEnumerable<Notification> Notifications { get; }
}

public interface INotificationManagement
{
    void AddNotification(Notification notification);
    void DismissNotification(string message);
    void DismissNotifications();
}

public class NotificationRepository : INotificationRepository, INotificationManagement
{
    public IEnumerable<Notification> Notifications => notifications;

    private readonly List<Notification> notifications = [];

    public void AddNotification(Notification notification)
    {
        notifications.RemoveAll(x => x.Message == notification.Message);
        notifications.Add(notification);
        Messenger.Send<NotificationsUpdated>();
    }

    public void DismissNotification(string message)
    {
        var numRemoved = notifications.RemoveAll(x => x.Message == message);
        if (numRemoved > 0)
        {
            Messenger.Send<NotificationsUpdated>();
        }
    }

    public void DismissNotifications()
    {
        if (notifications.Count > 0)
        {
            notifications.Clear();
            Messenger.Send<NotificationsUpdated>();
        }
    }
}
