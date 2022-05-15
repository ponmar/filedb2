using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Notifiers
{
    public class CacheNotifier : INotifier
    {
        private readonly string cacheDirectory;
        private readonly IEnumerable<int> cacheFileIds;

        public CacheNotifier(string cacheDirectory, IEnumerable<int> cacheFileIds)
        {
            this.cacheDirectory = cacheDirectory;
            this.cacheFileIds = cacheFileIds;
        }

        public List<Notification> Run()
        {
            List<Notification> notifications = new();
            if (!Directory.Exists(cacheDirectory))
            {
                notifications.Add(new Notification(NotificationType.Warning, $"File cache directory is missing: {cacheDirectory}", DateTime.Now));
            }
            else
            {
                var cachedFiles = new List<string>(Directory.GetFiles(cacheDirectory));

                foreach (var cacheFileId in cacheFileIds)
                {
                    // Note: file extension not included in tested path
                    if (!cachedFiles.Any(x => x.StartsWith($"{cacheFileId}.")))
                    {
                        notifications.Add(new Notification(NotificationType.Warning, $"File with id={cacheFileId} not in cache", DateTime.Now));
                    }
                }
            }
            return notifications;
        }
    }
}
