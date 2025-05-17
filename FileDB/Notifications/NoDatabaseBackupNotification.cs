using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class NoDatabaseBackupNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Warning;
    public string Message => Strings.BackupNotifierNoDatabaseBackupHasBeenCreated;
    public DateTime DateTime { get; } = DateTime.Now;
}
