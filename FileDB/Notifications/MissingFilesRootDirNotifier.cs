using System.Collections.Generic;
using System.IO;

namespace FileDB.Notifications
{
    public class MissingFilesRootDirNotifier : Notifier
    {
        private readonly string rootDirectory;

        public MissingFilesRootDirNotifier(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public List<Notification> GetNotifications()
        {
            List<Notification> notifications = new();
            if (!Directory.Exists(rootDirectory))
            {
                notifications.Add(new Notification(NotificationType.Warning, $"Files root directory is missing: {rootDirectory}"));
            }
            return notifications;
        }
    }
}
