using FileDB.Model;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileDB.Notifiers;

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
            notifications.Add(new Notification(NotificationType.Error, $"Database is missing ({filePaths.DatabasePath}). Restore it manually or create a new database at the Tools tab.", DateTime.Now));
        }
        return notifications;
    }
}
