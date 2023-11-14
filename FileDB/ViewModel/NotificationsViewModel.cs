using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Notifiers;

namespace FileDB.ViewModel;

public partial class NotificationsViewModel : ObservableObject
{
    public ObservableCollection<Notification> Notifications { get; } = [];

    private readonly DispatcherTimer notifierTimer = new();

    private readonly IConfigProvider configProvider;
    private readonly IDbAccessProvider dbAccessProvider;
    private readonly INotifierFactory notifierFactory;
    private readonly INotificationHandling notificationHandling;
    private readonly INotificationsRepository notificationsRepository;

    public NotificationsViewModel(IConfigProvider configProvider, IDbAccessProvider dbAccessProvider, INotifierFactory notifierFactory, INotificationHandling notificationHandling, INotificationsRepository notificationsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.notifierFactory = notifierFactory;
        this.notificationHandling = notificationHandling;
        this.notificationsRepository = notificationsRepository;

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
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
        var notifiers = notifierFactory.GetContinousNotifiers(configProvider.Config, dbAccessProvider.DbAccess);
        RunNotifiers(notifiers);
    }

    private void RunAllNotifiers()
    {
        var notifiers = notifierFactory.GetContinousNotifiers(configProvider.Config, dbAccessProvider.DbAccess);
        notifiers.AddRange(notifierFactory.GetStartupNotifiers(configProvider.Config, dbAccessProvider.DbAccess));
        RunNotifiers(notifiers);
    }

    private void RunNotifiers(List<INotifier> notifiers)
    {
        notifiers.ForEach(x => x.Run().ForEach(y => notificationHandling.AddNotification(y)));
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        foreach (var notification in notificationsRepository.Notifications)
        {
            Notifications.Add(notification);
        }
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        notificationHandling.DismissNotifications();
    }
}
