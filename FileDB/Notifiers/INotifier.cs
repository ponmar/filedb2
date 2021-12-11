using System.Collections.Generic;

namespace FileDB.Notifiers
{
    public enum NotificationType { Info, Warning, Error };

    public record Notification(NotificationType Type, string Message);

    interface INotifier
    {
        List<Notification> GetNotifications();
    }
}
