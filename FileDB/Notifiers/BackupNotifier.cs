using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileDB.ViewModel;

namespace FileDB.Notifiers
{
    public class BackupNotifier : Notifier
    {
        private readonly int afterDays;
        private readonly IEnumerable<BackupFile> backupFiles;

        public BackupNotifier(IEnumerable<BackupFile> backupFiles, int afterDays = 30)
        {
            this.backupFiles = backupFiles;
            this.afterDays = afterDays;
        }

        public List<Notification> GetNotifications()
        {
            List<Notification> notifications = new();
            if (!backupFiles.Any())
            {
                notifications.Add(new Notification(NotificationType.Warning, "Backup reminder: No database backup has been created!"));
            }
            else
            {
                var latestBackupDaysAge = (int)backupFiles.Min(x => x.Age).TotalDays;
                if (latestBackupDaysAge >= afterDays)
                {
                    notifications.Add(new Notification(NotificationType.Warning, $"Backup reminder: Last database backup created {latestBackupDaysAge} days ago!"));
                }
            }

            return notifications;
        }
    }
}
