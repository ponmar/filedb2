using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;

namespace FileDB.Model;

public interface INotificationHandling
{
    List<Notification> Notifications { get; }
    void AddNotification(Notification notification);
    void ClearNotifications();
}

public interface IDbAccessRepository
{
    IDbAccess DbAccess { get; }
}

public interface IFilesystemAccessRepository
{
    IFilesystemAccess FilesystemAccess { get; }
}

public interface IConfigRepository
{
    Config Config { get; }
}

public interface IConfigUpdater
{
    void InitConfig(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess, INotifierFactory notifierFactory);
    void UpdateConfig(Config config);
}

public class Model : INotificationHandling, IConfigRepository, IConfigUpdater, IDbAccessRepository, IFilesystemAccessRepository
{
    public IDbAccess DbAccess { get; private set; }
    public IFilesystemAccess FilesystemAccess { get; private set; }
    public INotifierFactory NotifierFactory { get; private set; }

    private DateTime date = DateTime.Now;

    public List<Notification> Notifications { get; } = new();

    public Model()
    {
        var dateCheckerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        dateCheckerTimer.Tick += DateCheckerTimer_Tick;
        dateCheckerTimer.Start();
    }

    private void DateCheckerTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if (date.Date != now.Date)
        {
            date = now;
            Events.Send<DateChanged>();
        }
    }

    public void AddNotification(Notification notification)
    {
        Notifications.RemoveAll(x => x.Message == notification.Message);
        Notifications.Add(notification);
        Events.Send<NotificationsUpdated>();
    }

    public void AddNotification(NotificationType type, string message)
    {
        AddNotification(new Notification(type, message, DateTime.Now));
    }

    public void ClearNotifications()
    {
        if (Notifications.Count > 0)
        {
            Notifications.Clear();
            Events.Send<NotificationsUpdated>();
        }
    }

    public Config Config { get; private set; }

    public void InitConfig(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess, INotifierFactory notifierFactory)
    {
        Config = config;
        DbAccess = dbAccess;
        FilesystemAccess = filesystemAccess;
        NotifierFactory = notifierFactory;
        Events.Send(new ConfigLoaded(config));
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        DbAccess.Database = config.Database;
        FilesystemAccess.FilesRootDirectory = config.FilesRootDirectory;
        Events.Send(new ConfigLoaded(config));
    }
}
