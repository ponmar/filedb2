using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.Notifiers;

namespace FileDB.ViewModel;

public partial class NotificationsViewModel : ObservableObject
{
    public ObservableCollection<Notification> Notifications { get; } = new();

    private readonly Model.Model model = Model.Model.Instance;

    private readonly DispatcherTimer notifierTimer = new();

    public NotificationsViewModel()
    {
        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            RunAllNotifiers();
        });

        WeakReferenceMessenger.Default.Register<NotificationsUpdated>(this, (r, m) =>
        {
            LoadNotifications();
        });

        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
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
        var notifiers = model.NotifierFactory.GetContinousNotifiers();
        RunNotifiers(notifiers);
    }

    private void RunAllNotifiers()
    {
        var notifiers = model.NotifierFactory.GetContinousNotifiers();
        notifiers.AddRange(model.NotifierFactory.GetStartupNotifiers());
        RunNotifiers(notifiers);
    }

    private void RunNotifiers(List<INotifier> notifiers)
    {
        notifiers.ForEach(x => x.Run().ForEach(y => model.AddNotification(y)));
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        model.Notifications.ForEach(x => Notifications.Add(x));
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        model.ClearNotifications();
    }
}
