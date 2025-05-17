using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class CollectionGetStartedNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message => Strings.SettingsUnsavedSettingsNotification;
    public DateTime DateTime { get; } = DateTime.Now;
}