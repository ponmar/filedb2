using System;

namespace FileDB.Notifications;

public enum NotificationSeverity { Info, Warning, Error };

public interface INotification
{
    NotificationSeverity Severity { get; }
    string Message { get; }
    DateTime DateTime { get; }
}
