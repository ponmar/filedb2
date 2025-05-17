using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Notifications;

namespace FileDB.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    public ObservableCollection<Notification> Notifications { get; } = [];

    private readonly INotificationManagement notificationManagement;
    private readonly INotificationRepository notificationRepo;

    public NotificationsViewModel(INotificationManagement notificationManagement, INotificationRepository notificationRepo)
    {
        this.notificationManagement = notificationManagement;
        this.notificationRepo = notificationRepo;

        this.RegisterForEvent<NotificationsUpdated>((x) =>
        {
            LoadNotifications();
        });

        LoadNotifications();
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        foreach (var notification in notificationRepo.Notifications)
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
