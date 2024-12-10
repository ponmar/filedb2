﻿using System;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModels.Search;

namespace FileDB.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int numNotifications = 0;

    [ObservableProperty]
    private NotificationType highlightedNotificationType;

    [ObservableProperty]
    private int numSearchResultFiles = 0;

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
    [NotifyPropertyChangedFor(nameof(Title))]
    private bool readWriteMode;

    [ObservableProperty]
    private SystemDecorations systemDecorations;

    [ObservableProperty]
    private WindowState windowState;

    [ObservableProperty]
    private bool fullscreen;

    private readonly IConfigProvider configProvider;
    private readonly INotificationsRepository notificationsRepository;
    private readonly ISearchResultRepository searchResultRepository;

    public MainViewModel(IConfigProvider configProvider, INotificationsRepository notificationsRepository, ISearchResultRepository searchResultRepository)
    {
        this.configProvider = configProvider;
        this.notificationsRepository = notificationsRepository;
        this.searchResultRepository = searchResultRepository;

        readWriteMode = !configProvider.Config.ReadOnly;

        NumNotifications = notificationsRepository.Notifications.Count();
        HighlightedNotificationType = NotificationsToType();

        var defaultWindowMode = configProvider.Config.WindowMode;
        ApplyWindowMode(configProvider.Config.WindowMode);

        this.RegisterForEvent<NotificationsUpdated>((x) =>
        {
            NumNotifications = notificationsRepository.Notifications.Count();
            HighlightedNotificationType = NotificationsToType();
        });

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !configProvider.Config.ReadOnly;
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
        SystemDecorations = windowMode == WindowMode.Fullscreen ? SystemDecorations.None : SystemDecorations.Full;
        WindowState = windowMode.ToWindowState();
        Fullscreen = windowMode == WindowMode.Fullscreen;
    }

    private NotificationType NotificationsToType()
    {
        return notificationsRepository.Notifications.Any() ? notificationsRepository.Notifications.Max(x => x.Type) : Enum.GetValues<NotificationType>().First();
    }

    [RelayCommand]
    private static void FunctionKeyPressed(string functionKeyStr)
    {
        var functionKey = int.Parse(functionKeyStr);
        Messenger.Send(new CategorizationFunctionKeyPressed(functionKey));
    }
}