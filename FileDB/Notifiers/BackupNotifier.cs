using FileDB.Notifications;
using FileDB.Services;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.Notifiers;

public class BackupNotifier : INotifier
{
    private readonly int afterDays;
    private readonly IFileBackup fileBackup;
    private readonly string filePath;

    public BackupNotifier(IFileBackup fileBackup, string filePath, int afterDays)
    {
        this.fileBackup = fileBackup;
        this.filePath = filePath;
        this.afterDays = afterDays;
    }

    public IEnumerable<INotification> Run()
    {
        var backupFiles = fileBackup.ListAvailableBackupFiles(filePath);
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

