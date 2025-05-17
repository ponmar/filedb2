using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class DatabaseMigrationNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public DatabaseMigrationNotification(int fromVersion, int toVersion)
    {
        Message = string.Format(Strings.NotificationDatabaseMigration, fromVersion, toVersion);
    }
}