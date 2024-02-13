using FileDBAvalonia.Lang;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDBAvalonia.Notifiers;

public class BackupNotifier : INotifier
{
    private readonly int afterDays;
    private readonly FileBackup fileBackup;

    public BackupNotifier(FileBackup fileBackup, int afterDays = 30)
    {
        this.fileBackup = fileBackup;
        this.afterDays = afterDays;
    }

    public List<Notification> Run()
    {
        List<Notification> notifications = [];
        var backupFiles = fileBackup.ListAvailableBackupFiles();
        if (!backupFiles.Any())
        {
            notifications.Add(new Notification(NotificationType.Warning, Strings.BackupNotifierNoDatabaseBackupHasBeenCreated, DateTime.Now));
        }
        else
        {
            var latestBackupDaysAge = (int)backupFiles.Min(x => x.Age).TotalDays;
            if (latestBackupDaysAge >= afterDays)
            {
                notifications.Add(new Notification(NotificationType.Warning, string.Format(Strings.BackupNotifierLongTimeSinceBackup, latestBackupDaysAge), DateTime.Now));
            }
        }

        return notifications;
    }
}
