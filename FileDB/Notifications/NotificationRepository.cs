using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using FileDB.Model;
using FileDB.Notifiers;

namespace FileDB.Notifications;

public interface INotificationRepository
{
    IEnumerable<INotification> Notifications { get; }
}

public interface INotificationManagement
{
    void AddNotification(INotification notification);
    void DismissNotification(string message);
    void DismissNotifications();
}

public class NotificationRepository : INotificationRepository, INotificationManagement
{
    public IEnumerable<INotification> Notifications => notifications;

    private readonly List<INotification> notifications = [];

    private readonly DispatcherTimer notifierTimer = new();

    private readonly INotifierFactory notifierFactory;
    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;

    public NotificationRepository(INotifierFactory notifierFactory, IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider)
    {
        this.notifierFactory = notifierFactory;
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            RunAllNotifiers();
        });

        this.RegisterForEvent<PersonsUpdated>((x) =>
        {
            RunContinousNotifiers();
        });

        RunAllNotifiers();

        notifierTimer.Tick += NotifierTimer_Tick;
        notifierTimer.Interval = TimeSpan.FromMinutes(5);
        notifierTimer.Start();
    }

    public void AddNotification(INotification notification)
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

    public void Close()
    {
        notifierTimer.Stop();
    }

    private void NotifierTimer_Tick(object? sender, EventArgs e)
    {
        RunContinousNotifiers();
    }

    private void RunContinousNotifiers()
    {
        var notifiers = notifierFactory.GetContinousNotifiers(configProvider.Config, dbAccessProvider.DbAccess);
        RunNotifiers(notifiers);
    }

    private void RunAllNotifiers()
    {
        var notifiers1 = notifierFactory.GetContinousNotifiers(configProvider.Config, dbAccessProvider.DbAccess);
        var notifiers2 = notifierFactory.GetStartupNotifiers(configProvider.Config, dbAccessProvider.DbAccess);
        RunNotifiers(notifiers1.Concat(notifiers2));
    }

    private void RunNotifiers(IEnumerable<INotifier> notifiers)
    {
        var notification = notifiers.SelectMany(x => x.Run()).ToList();
        notification.ForEach(AddNotification);
    }
}
