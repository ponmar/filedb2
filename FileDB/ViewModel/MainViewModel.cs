using System;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;

namespace FileDB.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int numNotifications = 0;

    [ObservableProperty]
    private NotificationType highlightedNotificationType;

    public string Title
    {
        get
        {
            var title = $"{Utils.ApplicationName} {Utils.GetVersionString()}";
            if (!string.IsNullOrEmpty(config.Name))
            {
                title += $" [{config.Name}]";
            }
            if (!ReadWriteMode)
            {
                title += " (read only)";
            }
            return title;
        }
    }

    [ObservableProperty]
    private WindowState windowState = DefaultWindowState;

    private static WindowState DefaultWindowState => Model.Model.Instance.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

    [ObservableProperty]
    private WindowStyle windowStyle = DefaultWindowStyle;

    private static WindowStyle DefaultWindowStyle => Model.Model.Instance.Config.WindowMode == WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    private Config config;
    private readonly INotificationHandling notificationHandling;

    public MainViewModel(Config config, INotificationHandling notificationHandling)
    {
        this.config = config;
        this.notificationHandling = notificationHandling;

        NumNotifications = notificationHandling.Notifications.Count;
        HighlightedNotificationType = NotificationsToType();

        WeakReferenceMessenger.Default.Register<NotificationsUpdated>(this, (r, m) =>
        {
            NumNotifications = notificationHandling.Notifications.Count;
            HighlightedNotificationType = NotificationsToType();
        });

        WeakReferenceMessenger.Default.Register<FullscreenBrowsingRequested>(this, (r, m) =>
        {
            WindowState = m.Fullscreen ? WindowState.Maximized : DefaultWindowState;
            WindowStyle = m.Fullscreen ? WindowStyle.None : DefaultWindowStyle;
        });

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            this.config = m.Config;
            ReadWriteMode = !this.config.ReadOnly;
            OnPropertyChanged(nameof(Title));
        });
    }

    private NotificationType NotificationsToType()
    {
        return notificationHandling.Notifications.Count > 0 ? notificationHandling.Notifications.Max(x => x.Type) : Enum.GetValues<NotificationType>().First();
    }
}
