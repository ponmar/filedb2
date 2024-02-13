using FileDBAvalonia.Model;
using FileDBAvalonia.Lang;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileDBAvalonia.Notifiers;

public class MissingDatabaseNotifier : INotifier
{
    private readonly ApplicationFilePaths filePaths;
    private readonly IFileSystem fileSystem;

    public MissingDatabaseNotifier(ApplicationFilePaths filePaths, IFileSystem fileSystem)
    {
        this.filePaths = filePaths;
        this.fileSystem = fileSystem;
    }

    public List<Notification> Run()
    {
        var notifications = new List<Notification>();
        if (!fileSystem.File.Exists(filePaths.DatabasePath))
        {
            notifications.Add(new Notification(NotificationType.Error, string.Format(Strings.NotificationDatabaseIsMissing, filePaths.DatabasePath), DateTime.Now));
        }
        return notifications;
    }
}
