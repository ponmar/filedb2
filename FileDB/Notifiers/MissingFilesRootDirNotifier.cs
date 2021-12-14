using System;
using System.Collections.Generic;
using System.IO;

namespace FileDB.Notifiers
{
    public class MissingFilesRootDirNotifier : INotifier
    {
        private readonly string rootDirectory;

        public MissingFilesRootDirNotifier(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public List<Notification> Run()
        {
            List<Notification> notifications = new();
            if (!Directory.Exists(rootDirectory))
            {
                notifications.Add(new Notification(NotificationType.Warning, $"Files root directory is missing: {rootDirectory}", DateTime.Now));
            }
            return notifications;
        }
    }
}
