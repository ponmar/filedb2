using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class CollectionDemoUsedNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message => Strings.StartupNotificationDemoConfigurationEnabled;
    public DateTime DateTime { get; } = DateTime.Now;
}