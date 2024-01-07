﻿using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;

namespace FileDB.Model;

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

public interface IDbAccessProvider
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

public class Model : INotificationHandling, INotificationsRepository, IConfigProvider, IConfigUpdater, IDbAccessProvider, IFilesystemAccessProvider
{
    public IDatabaseAccess DbAccess { get; private set; } = new NoDbAccess();
    public IFilesystemAccess FilesystemAccess { get; private set; }
    public INotifierFactory NotifierFactory { get; } = new NotifierFactory();
    public ApplicationFilePaths FilePaths { get; private set; }

    private DateTime date = DateTime.Now;

    public Config Config { get; private set; }

    public IEnumerable<Notification> Notifications => notifications;
    private readonly List<Notification> notifications = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Model()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
        // TODO: move to a DateObserver class
        var now = DateTime.Now;
        if (date.Date != now.Date)
        {
            date = now;
            Events.Send<DateChanged>();
        }
    }

    public void AddNotification(Notification notification)
    {
        notifications.RemoveAll(x => x.Message == notification.Message);
        notifications.Add(notification);
        Events.Send<NotificationsUpdated>();
    }

    public void DismissNotification(string message)
    {
        var numRemoved = notifications.RemoveAll(x => x.Message == message);
        if (numRemoved > 0)
        {
            Events.Send<NotificationsUpdated>();
        }
    }

    public void DismissNotifications()
    {
        if (notifications.Count > 0)
        {
            notifications.Clear();
            Events.Send<NotificationsUpdated>();
        }
    }

    public void InitConfig(ApplicationFilePaths filePaths, Config config, IDatabaseAccess dbAccess, IFilesystemAccess filesystemAccess)
    {
        FilePaths = filePaths;
        Config = config;
        DbAccess = dbAccess;
        FilesystemAccess = filesystemAccess;
        Events.Send<ConfigUpdated>();
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        Events.Send<ConfigUpdated>();
    }
}
