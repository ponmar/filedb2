using System.Collections.Generic;
using System.Collections.ObjectModel;
using FileDB.Notifiers;

namespace FileDB.ViewModel
{
    public class NotificationsViewModel : ViewModelBase
    {
        public ObservableCollection<Notification> Notifications { get; } = new();

        public NotificationsViewModel()
        {
            var notifiers = new List<INotifier>();

            if (Utils.Config.BackupReminder)
            {
                notifiers.Add(new BackupNotifier(new DatabaseBackup().ListAvailableBackupFiles(), 30));
            }

            if (Utils.Config.MissingFilesRootDirNotification)
            {
                notifiers.Add(new MissingFilesRootDirNotifier(Utils.Config.FilesRootDirectory));
            }

            var persons = Utils.DbAccess.GetPersons();

            if (Utils.Config.BirthdayReminder)
            {
                notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Alive));
            }

            if (Utils.Config.BirthdayReminderForDeceased)
            {
                notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Deceased));
            }

            if (Utils.Config.RipReminder)
            {
                notifiers.Add(new RestInPeaceNotifier(persons));
            }

            notifiers.ForEach(x => AddNotifications(x.GetNotifications()));
        }

        private void AddNotifications(List<Notification> notifications)
        {
            notifications.ForEach(x => Notifications.Add(x));
        }
    }
}
