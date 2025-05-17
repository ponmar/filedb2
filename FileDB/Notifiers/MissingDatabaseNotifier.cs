using FileDB.Model;
using FileDB.Lang;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using FileDB.Notifications;

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

    public List<INotification> Run()
    {
        var notifications = new List<INotification>();
        if (!fileSystem.File.Exists(filePaths.DatabasePath))
        {
            notifications.Add(new DatabaseMissingNotification(filePaths.DatabasePath));
        }
        return notifications;
    }
}
