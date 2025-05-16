using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Notifiers;

namespace FileDB.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    public ObservableCollection<Notification> Notifications { get; } = [];

    private readonly DispatcherTimer notifierTimer = new();

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly INotifierFactory notifierFactory;
    private readonly INotificationManagement notificationManagement;
    private readonly INotificationRepository notificationRepository;

    public NotificationsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, INotifierFactory notifierFactory, INotificationManagement notificationManagement, INotificationRepository notificationRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.notifierFactory = notifierFactory;
        this.notificationManagement = notificationManagement;
        this.notificationRepository = notificationRepository;

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
        notifiers.ForEach(x => x.Run().ForEach(y => notificationManagement.AddNotification(y)));
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        foreach (var notification in notificationRepository.Notifications)
        {
            Notifications.Add(notification);
        }
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        notificationManagement.DismissNotifications();
    }
}
