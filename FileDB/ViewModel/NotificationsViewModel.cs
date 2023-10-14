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
    public ObservableCollection<Notification> Notifications { get; } = new();

    private readonly DispatcherTimer notifierTimer = new();

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly INotifierFactory notifierFactory;
    private readonly INotificationHandling notificationHandling;
    private readonly INotificationsRepository notificationsRepository;

    public NotificationsViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, INotifierFactory notifierFactory, INotificationHandling notificationHandling, INotificationsRepository notificationsRepository)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
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
        var notifiers = notifierFactory.GetContinousNotifiers(configRepository.Config, dbAccessRepository.DbAccess);
        RunNotifiers(notifiers);
    }

    private void RunAllNotifiers()
    {
        var notifiers = notifierFactory.GetContinousNotifiers(configRepository.Config, dbAccessRepository.DbAccess);
        notifiers.AddRange(notifierFactory.GetStartupNotifiers(configRepository.Config, dbAccessRepository.DbAccess));
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
