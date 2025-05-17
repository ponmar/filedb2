using System;

namespace FileDB.Notifications;

public class DatabaseMissingNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Error;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public DatabaseMissingNotification(string databasePath)
    {
        Message = string.Format(Lang.Strings.NotificationDatabaseIsMissing, databasePath);
    }
}