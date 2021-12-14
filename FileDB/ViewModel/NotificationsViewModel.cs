using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FileDB.Notifiers;

namespace FileDB.ViewModel
{
    public class NotificationsViewModel : ViewModelBase
    {
        public ObservableCollection<Notification> Notifications { get; } = new();

        public ICommand ClearNotificationsCommand => clearNotificationsCommand ??= new CommandHandler(() => Model.Model.Instance.ClearNotifications());
        private ICommand clearNotificationsCommand;

        public NotificationsViewModel()
        {
            var model = Model.Model.Instance;
            model.NotificationsUpdated += Model_NotificationsUpdated;

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


            notifiers.ForEach(x => x.Run().ForEach(y => model.AddNotification(y)));

            // TODO: remove test
            model.AddNotification(new Notification(NotificationType.Error, "Test"));
        }

        private void Model_NotificationsUpdated(object sender, System.EventArgs e)
        {
            Notifications.Clear();
            Model.Model.Instance.Notifications.ForEach(x => Notifications.Add(x));
            OnPropertyChanged(nameof(Notifications));
        }
    }
}
