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

        private readonly Model.Model model = Model.Model.Instance;

        public NotificationsViewModel()
        {
            var model = Model.Model.Instance;
            model.NotificationsUpdated += Model_NotificationsUpdated;
            SetNotifications();

            var notifiers = new List<INotifier>();

            if (model.Config.BackupReminder)
            {
                notifiers.Add(new BackupNotifier(new DatabaseBackup().ListAvailableBackupFiles(), 30));
            }

            if (model.Config.MissingFilesRootDirNotification)
            {
                notifiers.Add(new MissingFilesRootDirNotifier(model.Config.FilesRootDirectory));
            }

            var persons = model.DbAccess.GetPersons();

            if (model.Config.BirthdayReminder)
            {
                notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Alive));
            }

            if (model.Config.BirthdayReminderForDeceased)
            {
                notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Deceased));
            }

            if (model.Config.RipReminder)
            {
                notifiers.Add(new RestInPeaceNotifier(persons));
            }

            notifiers.ForEach(x => x.Run().ForEach(y => model.AddNotification(y)));
        }

        private void Model_NotificationsUpdated(object sender, System.EventArgs e)
        {
            SetNotifications();
        }

        private void SetNotifications()
        {
            Notifications.Clear();
            Model.Model.Instance.Notifications.ForEach(x => Notifications.Add(x));
            OnPropertyChanged(nameof(Notifications));
        }
    }
}
