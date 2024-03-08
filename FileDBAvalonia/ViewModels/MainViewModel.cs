using System;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;

namespace FileDBAvalonia.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int numNotifications = 0;

    [ObservableProperty]
    private NotificationType highlightedNotificationType;

    [ObservableProperty]
    private bool quitSelected;

    partial void OnQuitSelectedChanged(bool value)
    {
        if (value)
        {
            Messenger.Send<Quit>();
        }
    }

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

    [ObservableProperty]
    private SystemDecorations systemDecorations;

    [ObservableProperty]
    private bool topmost;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private bool readWriteMode;

    private IConfigProvider configProvider;
    private readonly INotificationsRepository notificationsRepository;

    public MainViewModel(IConfigProvider configProvider, INotificationsRepository notificationsRepository)
    {
        this.configProvider = configProvider;
        this.notificationsRepository = notificationsRepository;

        var defaultWindowState = configProvider.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;

        SystemDecorations = configProvider.Config.WindowMode == WindowMode.Fullscreen ? SystemDecorations.None : SystemDecorations.Full;
        WindowState = configProvider.Config.WindowMode == WindowMode.Normal ? WindowState.Normal : WindowState.Maximized;
        Topmost = configProvider.Config.WindowMode == WindowMode.Fullscreen;

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
            SystemDecorations = x.Fullscreen ? SystemDecorations.None : SystemDecorations.Full;
            WindowState = x.Fullscreen ? WindowState.Maximized : defaultWindowState;
            Topmost = x.Fullscreen;
        });

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configProvider.Config.ReadOnly;
            OnPropertyChanged(nameof(Title));
        });
    }

    private NotificationType NotificationsToType()
    {
        return notificationsRepository.Notifications.Any() ? notificationsRepository.Notifications.Max(x => x.Type) : Enum.GetValues<NotificationType>().First();
    }
}
