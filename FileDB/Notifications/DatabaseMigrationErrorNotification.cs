using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class DatabaseMigrationErrorNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Error;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public DatabaseMigrationErrorNotification(int fromVersion, int toVersion, string error)
    {
        Message = string.Format(Strings.NotificationDatabaseMigrationError, fromVersion, toVersion, error);
    }
}
