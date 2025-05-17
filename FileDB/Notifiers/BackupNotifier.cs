using FileDB.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.Notifiers;

public class BackupNotifier : INotifier
{
    private readonly int afterDays;
    private readonly FileBackup fileBackup;

    public BackupNotifier(FileBackup fileBackup, int afterDays)
    {
        this.fileBackup = fileBackup;
        this.afterDays = afterDays;
    }

    public IEnumerable<INotification> Run()
    {
        var backupFiles = fileBackup.ListAvailableBackupFiles();
        if (backupFiles.Count == 0)
        {
            return [new DatabaseBackupMissingNotification()];
        }
        
        var latestBackupDaysAge = (int)backupFiles.Min(x => x.Age).TotalDays;
        if (latestBackupDaysAge >= afterDays)
        {
            return [new DatabaseBackupTooLongTimeAgoNotification(latestBackupDaysAge)];
        }

        return [];
    }
}
