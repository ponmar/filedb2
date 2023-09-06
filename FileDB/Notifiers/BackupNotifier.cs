using FileDB.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.Notifiers;

public class BackupNotifier : INotifier
{
    private readonly int afterDays;
    private readonly IEnumerable<BackupFile> backupFiles;

    public BackupNotifier(IEnumerable<BackupFile> backupFiles, int afterDays = 30)
    {
        this.backupFiles = backupFiles;
        this.afterDays = afterDays;
    }

    public List<Notification> Run()
    {
        List<Notification> notifications = new();
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
