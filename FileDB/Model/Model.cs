using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using FileDB.FileBrowsingPlugins;
using CommunityToolkit.Mvvm.Messaging;
using FileDBInterface.DbAccess.SQLite;

namespace FileDB.Model;

public class Model
{
    public static Model Instance => instance ??= new();
    private static Model? instance;

    private readonly List<IBrowsingPlugin> browsingPlugins = new();

    private Model()
    {
        var dateCheckerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        dateCheckerTimer.Tick += DateCheckerTimer_Tick;
        dateCheckerTimer.Start();
    }

    public void StartFileBrowsingPlugins()
    {
    }

    private DateTime date = DateTime.Now;

    private void DateCheckerTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if (date.Date != now.Date)
        {
            date = now;
            WeakReferenceMessenger.Default.Send(new DateChanged());
        }
    }

    public List<Notification> Notifications { get; } = new();

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

        /*
        if (DbAccess == null)
        {
            try
            {
                DbAccess = new SqLiteDbAccess(config.Database);
            }
            catch (Exception e)
            {
                AddNotification(new Notification(NotificationType.Error, e.Message, DateTime.Now));
                DbAccess = new NoDbAccess();
            }
        }
        else
        {
            DbAccess.Database = config.Database;
        }

        if (FilesystemAccess == null)
        {
            FilesystemAccess = new FilesystemAccess(config.FilesRootDirectory);
        }
        else
        {
            FilesystemAccess.FilesRootDirectory = config.FilesRootDirectory;
        }

        NotifierFactory = new NotifierFactory();
        */

        WeakReferenceMessenger.Default.Send(new ConfigLoaded());
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        DbAccess.Database = config.Database;
        FilesystemAccess.FilesRootDirectory = config.FilesRootDirectory;

        WeakReferenceMessenger.Default.Send(new ConfigLoaded());
    }

    public IDbAccess DbAccess { get; private set; }

    public IFilesystemAccess FilesystemAccess { get; private set; }

    public INotifierFactory NotifierFactory { get; private set; }

    public void FileLoaded(FilesModel file)
    {
        browsingPlugins.ForEach(x => x.FileLoaded(file));
    }
}
