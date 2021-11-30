﻿using System.Collections.Generic;

namespace FileDB.Notifications
{
    public enum NotificationType { Info, Warning, Error };

    public record Notification(NotificationType Type, string Message);

    interface Notifier
    {
        List<Notification> GetNotifications();
    }
}
