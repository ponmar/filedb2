using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using CommunityToolkit.Mvvm.Messaging;

namespace FileDB.Model;

public interface INotificationHandling
{
    List<Notification> Notifications { get; }
    void AddNotification(Notification notification);
    void ClearNotifications();
}

public interface IConfigSaver
{
    void UpdateConfig(Config config);
}

public class Model : INotificationHandling, IConfigSaver
{
    public static Model Instance => instance ??= new();
    private static Model? instance;

    public IDbAccess DbAccess { get; private set; }
    public IFilesystemAccess FilesystemAccess { get; private set; }
    public INotifierFactory NotifierFactory { get; private set; }
    public IDialogs Dialogs { get; } = new Dialogs();

    private DateTime date = DateTime.Now;

    public List<Notification> Notifications { get; } = new();

    private Model()
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
            WeakReferenceMessenger.Default.Send(new DateChanged());
        }
    }

    public void AddNotification(Notification notification)
    {
        Notifications.RemoveAll(x => x.Message == notification.Message);
        Notifications.Add(notification);
        WeakReferenceMessenger.Default.Send(new NotificationsUpdated());
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
            WeakReferenceMessenger.Default.Send(new NotificationsUpdated());
        }
    }

    public Config Config { get; private set; }

    public void InitConfig(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess, INotifierFactory notifierFactory)
    {
        Config = config;
        DbAccess = dbAccess;
        FilesystemAccess = filesystemAccess;
        NotifierFactory = notifierFactory;

        WeakReferenceMessenger.Default.Send(new ConfigLoaded(config));
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        DbAccess.Database = config.Database;
        FilesystemAccess.FilesRootDirectory = config.FilesRootDirectory;

        WeakReferenceMessenger.Default.Send(new ConfigLoaded(config));
    }
}
