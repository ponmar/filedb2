using FileDB.Lang;
using FileDB.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.Notifiers;

public class BackupNotifier : INotifier
{
    private readonly int afterDays;
    private readonly FileBackup fileBackup;

    public BackupNotifier(FileBackup fileBackup, int afterDays = 30)
    {
        this.fileBackup = fileBackup;
        this.afterDays = afterDays;
    }

    public List<INotification> Run()
    {
        List<INotification> notifications = [];
        var backupFiles = fileBackup.ListAvailableBackupFiles();
        if (!backupFiles.Any())
        {
            notifications.Add(new NoDatabaseBackupNotification());
        }
        else
        {
            var latestBackupDaysAge = (int)backupFiles.Min(x => x.Age).TotalDays;
            if (latestBackupDaysAge >= afterDays)
            {
                notifications.Add(new TooLongTimeSinceDatabaseBackupNotification(latestBackupDaysAge));
            }
        }

        return notifications;
    }
}
