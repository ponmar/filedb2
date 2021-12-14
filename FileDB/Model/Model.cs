using System;
using System.Collections.Generic;
using FileDB.Notifiers;

namespace FileDB.Model
{
    public class Model
    {
        public static Model Instance => instance ??= new();
        private static Model instance;

        public List<Notification> Notifications { get; } = new();

        public event EventHandler NotificationsUpdated;

        public void AddNotification(Notification notification)
        {
            Notifications.Add(notification);
            NotificationsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void ClearNotifications()
        {
            if (Notifications.Count > 0)
            {
                Notifications.Clear();
                NotificationsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
