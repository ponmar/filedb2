using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Notifiers;

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
            var cachedFiles = new List<string>(Directory.GetFiles(cacheDirectory)).Select(x => Path.GetFileName(x));
            var fileIdsMissingInCache = cacheFileIds.Where(x => !cachedFiles.Contains(x.ToString())).ToList();
            if (fileIdsMissingInCache.Count > 0)
            {
                notifications.Add(new Notification(NotificationType.Warning, $"Files missing in cache: {string.Join(", ", fileIdsMissingInCache)}", DateTime.Now));
            }
        }
        return notifications;
    }
}
