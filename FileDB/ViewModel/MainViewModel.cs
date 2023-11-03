using System;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
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
            var title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - {configRepository.FilePaths.ConfigPath}";
            if (!ReadWriteMode)
            {
                title += " (read only)";
            }
            return title;
        }
    }

    [ObservableProperty]
    private WindowState windowState;

    private readonly WindowState defaultWindowState;

    [ObservableProperty]
    private WindowStyle windowStyle;

    private readonly WindowStyle defaultWindowStyle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private bool readWriteMode;

    private IConfigRepository configRepository;
    private readonly INotificationsRepository notificationsRepository;

    public MainViewModel(IConfigRepository configRepository, INotificationsRepository notificationsRepository)
    {
        this.configRepository = configRepository;
        this.notificationsRepository = notificationsRepository;

        defaultWindowState = configRepository.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;
        defaultWindowStyle = configRepository.Config.WindowMode == WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

        windowState = defaultWindowState;
        windowStyle = defaultWindowStyle;

        readWriteMode = !configRepository.Config.ReadOnly;

        NumNotifications = notificationsRepository.Notifications.Count();
        HighlightedNotificationType = NotificationsToType();

        this.RegisterForEvent<NotificationsUpdated>((x) =>
        {
            NumNotifications = notificationsRepository.Notifications.Count();
            HighlightedNotificationType = NotificationsToType();
        });

        this.RegisterForEvent<FullscreenBrowsingRequested>((x) =>
        {
            WindowState = x.Fullscreen ? WindowState.Maximized : defaultWindowState;
            WindowStyle = x.Fullscreen ? WindowStyle.None : defaultWindowStyle;
        });

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configRepository.Config.ReadOnly;
            OnPropertyChanged(nameof(Title));
        });
    }

    private NotificationType NotificationsToType()
    {
        return notificationsRepository.Notifications.Count() > 0 ? notificationsRepository.Notifications.Max(x => x.Type) : Enum.GetValues<NotificationType>().First();
    }
}
