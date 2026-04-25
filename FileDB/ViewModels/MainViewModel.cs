using System;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.ViewModels.Search;

namespace FileDB.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int NumNotifications { get; set; } = 0;

    [ObservableProperty]
    public partial NotificationSeverity HighlightedNotificationSeverity { get; set; }

    [ObservableProperty]
    public partial int NumSearchResultFiles { get; set; } = 0;

    [ObservableProperty]
    public partial bool QuitSelected { get; set; }

    partial void OnQuitSelectedChanged(bool value)
    {
        if (value)
        {
            Messenger.Send<Quit>();
        }
    }

    [ObservableProperty]
    public partial bool SearchTabSelected { get; set; } = true;

    public string Title
    {
        get
        {
            var title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - {configProvider.FilePaths.ConfigPath}";
            if (ReadOnly)
            {
                title += " (read only)";
            }
            return title;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool ReadOnly { get; set; }

    [ObservableProperty]
    public partial WindowDecorations WindowDecorations { get; set; }

    [ObservableProperty]
    public partial WindowState WindowState { get; set; }

    [ObservableProperty]
    public partial bool Fullscreen { get; set; }

    private readonly IConfigProvider configProvider;
    private readonly INotificationRepository notificationRepository;
    private readonly ISearchResultRepository searchResultRepository;

    public MainViewModel(IConfigProvider configProvider, INotificationRepository notificationRepository, ISearchResultRepository searchResultRepository)
    {
        this.configProvider = configProvider;
        this.notificationRepository = notificationRepository;
        this.searchResultRepository = searchResultRepository;
        ReadOnly = configProvider.Config.ReadOnly;

        NumNotifications = notificationRepository.Notifications.Count();
        HighlightedNotificationSeverity = NotificationsToSeverity();

        var defaultWindowMode = configProvider.Config.WindowMode;
        ApplyWindowMode(configProvider.Config.WindowMode);

        this.RegisterForEvent<NotificationsUpdated>((x) =>
        {
            NumNotifications = notificationRepository.Notifications.Count();
            HighlightedNotificationSeverity = NotificationsToSeverity();
        });

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadOnly = configProvider.Config.ReadOnly;
            OnPropertyChanged(nameof(Title));
            ApplyWindowMode(configProvider.Config.WindowMode);
        });

        this.RegisterForEvent<FullscreenBrowsingRequested>(x =>
        {
            var windowMode = x.Fullscreen ? WindowMode.Fullscreen : defaultWindowMode;
            ApplyWindowMode(windowMode);
        });

        this.RegisterForEvent<SearchResultRepositoryUpdated>(x =>
        {
            NumSearchResultFiles = searchResultRepository.Files.Count();
        });
    }

    private void ApplyWindowMode(WindowMode windowMode)
    {
        WindowDecorations = windowMode == WindowMode.Fullscreen ? WindowDecorations.None : WindowDecorations.Full;
        WindowState = windowMode.ToWindowState();
        Fullscreen = windowMode == WindowMode.Fullscreen;
    }

    private NotificationSeverity NotificationsToSeverity()
    {
        return notificationRepository.Notifications.Any() ? notificationRepository.Notifications.Max(x => x.Severity) : Enum.GetValues<NotificationSeverity>().First();
    }

    [RelayCommand]
    private void FunctionKeyPressed(string functionKeyStr)
    {
        if (SearchTabSelected)
        {
            var functionKey = int.Parse(functionKeyStr);
            Messenger.Send(new CategorizationFunctionKeyPressed(functionKey));
        }
    }
}
