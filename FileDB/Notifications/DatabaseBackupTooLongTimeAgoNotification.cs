using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class DatabaseBackupTooLongTimeAgoNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Warning;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public DatabaseBackupTooLongTimeAgoNotification(int latestBackupDaysAge)
    {
        Message = string.Format(Strings.BackupNotifierLongTimeSinceBackup, latestBackupDaysAge);
    }
}