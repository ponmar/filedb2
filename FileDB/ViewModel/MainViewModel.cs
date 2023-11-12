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
            var title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - {configProvider.FilePaths.ConfigPath}";
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

    private IConfigProvider configProvider;
    private readonly INotificationsRepository notificationsRepository;

    public MainViewModel(IConfigProvider configProvider, INotificationsRepository notificationsRepository)
    {
        this.configProvider = configProvider;
        this.notificationsRepository = notificationsRepository;

        defaultWindowState = configProvider.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;
        defaultWindowStyle = configProvider.Config.WindowMode == WindowMode.Fullscreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;

        windowState = defaultWindowState;
        windowStyle = defaultWindowStyle;

        readWriteMode = !configProvider.Config.ReadOnly;

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
            ReadWriteMode = !configProvider.Config.ReadOnly;
            OnPropertyChanged(nameof(Title));
        });
    }

    private NotificationType NotificationsToType()
    {
        return notificationsRepository.Notifications.Count() > 0 ? notificationsRepository.Notifications.Max(x => x.Type) : Enum.GetValues<NotificationType>().First();
    }
}
