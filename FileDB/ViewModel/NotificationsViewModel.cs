using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;

namespace FileDB.ViewModel;

public partial class NotificationsViewModel : ObservableObject
{
    public ObservableCollection<Notification> Notifications { get; } = new();

    private readonly DispatcherTimer notifierTimer = new();

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly INotifierFactory notifierFactory;
    private readonly INotificationHandling notificationHandling;

    public NotificationsViewModel(Config config, IDbAccess dbAccess, INotifierFactory notifierFactory, INotificationHandling notificationHandling)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        this.notifierFactory = notifierFactory;
        this.notificationHandling = notificationHandling;

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            this.config = x.Config;
            RunAllNotifiers();
        });

        this.RegisterForEvent<NotificationsUpdated>((x) =>
        {
            LoadNotifications();
        });

        this.RegisterForEvent<PersonsUpdated>((x) =>
        {
            RunContinousNotifiers();
        });

        LoadNotifications();
        RunAllNotifiers();

        notifierTimer.Tick += NotifierTimer_Tick;
        notifierTimer.Interval = TimeSpan.FromMinutes(5);
        notifierTimer.Start();
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
        var notifiers = notifierFactory.GetContinousNotifiers(config, dbAccess);
        RunNotifiers(notifiers);
    }

    private void RunAllNotifiers()
    {
        var notifiers = notifierFactory.GetContinousNotifiers(config, dbAccess);
        notifiers.AddRange(notifierFactory.GetStartupNotifiers(config, dbAccess));
        RunNotifiers(notifiers);
    }

    private void RunNotifiers(List<INotifier> notifiers)
    {
        notifiers.ForEach(x => x.Run().ForEach(y => notificationHandling.AddNotification(y)));
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        notificationHandling.Notifications.ForEach(x => Notifications.Add(x));
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        notificationHandling.ClearNotifications();
    }
}
