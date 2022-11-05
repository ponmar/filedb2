using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using FileDBInterface.DbAccess.SQLite;
using FileDB.FileBrowsingPlugins;
using FileDB.Validators;
using CommunityToolkit.Mvvm.Messaging;

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
        var configValidator = new ConfigValidator();
        if (configValidator.CastingEnabled(Config))
        {
            FileCaster.StartServer(Config.CastHttpServerPort);
            browsingPlugins.Add(new Cast(Config.CastHttpServerInterface!, Config.CastHttpServerPort));
        }
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

    public Config Config
    {
        get => config;
        set
        {
            if (config != value)
            {
                config = value;
                WeakReferenceMessenger.Default.Send(new ConfigLoaded());
            }
        }
    }
    private Config config;

    public IDbAccess DbAccess
    {
        get
        {
            if (dbAccess == null)
            {
                ReloadHandles();
            }
            return dbAccess!;
        }
    }
    private IDbAccess? dbAccess;

    public void ReloadHandles()
    {
        try
        {
            dbAccess = new SqLiteDbAccess(Config.Database);
        }
        catch (DatabaseWrapperException)
        {
            dbAccess = new NoDbAccess();
        }

        filesystemAccess = new FilesystemAccess(Config.FilesRootDirectory);
    }

    public IFilesystemAccess FilesystemAccess
    {
        get
        {
            if (filesystemAccess == null)
            {
                ReloadHandles();
            }
            return filesystemAccess!;
        }
    }
    private IFilesystemAccess? filesystemAccess;

    public void FileLoaded(FilesModel file)
    {
        browsingPlugins.ForEach(x => x.FileLoaded(file));
    }
}
