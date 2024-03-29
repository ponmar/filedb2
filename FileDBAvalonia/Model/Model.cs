using System.Collections.Generic;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Notifiers;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;

namespace FileDBAvalonia.Model;

public interface INotificationsRepository
{
    IEnumerable<Notification> Notifications { get; }
}

public interface INotificationHandling
{ 
    void AddNotification(Notification notification);
    void DismissNotification(string message);
    void DismissNotifications();
}

public interface IDatabaseAccessProvider
{
    IDatabaseAccess DbAccess { get; }
}

public interface IFilesystemAccessProvider
{
    IFilesystemAccess FilesystemAccess { get; }
}

public interface IConfigProvider
{
    ApplicationFilePaths FilePaths { get; }
    Config Config { get; }
}

public interface IConfigUpdater
{
    void InitConfig(ApplicationFilePaths applicationFilePaths, Config config, IDatabaseAccess dbAccess, IFilesystemAccess filesystemAccess);
    void UpdateConfig(Config config);
}

public record ApplicationFilePaths(string FilesRootDir, string ConfigPath, string DatabasePath);

public class Model : INotificationHandling, INotificationsRepository, IConfigProvider, IConfigUpdater, IDatabaseAccessProvider, IFilesystemAccessProvider
{
    public IDatabaseAccess DbAccess { get; private set; } = new NoDatabaseAccess();
    public IFilesystemAccess FilesystemAccess { get; private set; }
    public INotifierFactory NotifierFactory { get; } = new NotifierFactory();
    public ApplicationFilePaths FilePaths { get; private set; }

    public Config Config { get; private set; }

    public IEnumerable<Notification> Notifications => notifications;
    private readonly List<Notification> notifications = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Model()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    public void AddNotification(Notification notification)
    {
        notifications.RemoveAll(x => x.Message == notification.Message);
        notifications.Add(notification);
        Messenger.Send<NotificationsUpdated>();
    }

    public void DismissNotification(string message)
    {
        var numRemoved = notifications.RemoveAll(x => x.Message == message);
        if (numRemoved > 0)
        {
            Messenger.Send<NotificationsUpdated>();
        }
    }

    public void DismissNotifications()
    {
        if (notifications.Count > 0)
        {
            notifications.Clear();
            Messenger.Send<NotificationsUpdated>();
        }
    }

    public void InitConfig(ApplicationFilePaths filePaths, Config config, IDatabaseAccess dbAccess, IFilesystemAccess filesystemAccess)
    {
        FilePaths = filePaths;
        Config = config;
        DbAccess = dbAccess;
        FilesystemAccess = filesystemAccess;
        Messenger.Send<ConfigUpdated>();
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        Messenger.Send<ConfigUpdated>();
    }
}
