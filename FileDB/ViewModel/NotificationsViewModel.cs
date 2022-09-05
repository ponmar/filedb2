using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Configuration;
using FileDB.Notifiers;

namespace FileDB.ViewModel
{
    public partial class NotificationsViewModel : ObservableObject
    {
        public ObservableCollection<Notification> Notifications { get; } = new();

        [RelayCommand]
        private void ClearNotifications()
        {
            Model.Model.Instance.ClearNotifications();
        }

        private readonly Model.Model model = Model.Model.Instance;

        private readonly DispatcherTimer notifierTimer = new();

        public NotificationsViewModel()
        {
            var model = Model.Model.Instance;
            model.NotificationsUpdated += Model_NotificationsUpdated;
            model.ConfigLoaded += Model_ConfigLoaded;

            LoadNotifications();
            RunAllNotifiers();

            notifierTimer.Tick += NotifierTimer_Tick;
            notifierTimer.Interval = TimeSpan.FromMinutes(5);
            notifierTimer.Start();
        }

        private void NotifierTimer_Tick(object? sender, EventArgs e)
        {
            RunContinousNotifiers();
        }

        private void Model_ConfigLoaded(object? sender, EventArgs e)
        {
            RunAllNotifiers();
        }

        private List<INotifier> GetContinousNotifiers()
        {
            var notifiers = new List<INotifier>();
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

            return notifiers;
        }

        private List<INotifier> GetStartupNotifiers()
        {
            var notifiers = new List<INotifier>();

            if (model.Config.BackupReminder)
            {
                notifiers.Add(new BackupNotifier(new DatabaseBackup().ListAvailableBackupFiles(), 30));
            }

            if (model.Config.MissingFilesRootDirNotification)
            {
                notifiers.Add(new MissingFilesRootDirNotifier(model.Config.FilesRootDirectory));
            }

            if (model.Config.CacheFiles)
            {
                var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
                var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);
                var cacheFileIds = model.DbAccess.GetPersons().Where(x => x.ProfileFileId != null).Select(x => x.ProfileFileId!.Value);
                notifiers.Add(new CacheNotifier(cacheDir, cacheFileIds));
            }

            return notifiers;
        }

        private void RunContinousNotifiers()
        {
            var notifiers = GetContinousNotifiers();
            RunNotifiers(notifiers);
        }

        private void RunAllNotifiers()
        {
            var notifiers = GetContinousNotifiers();
            notifiers.AddRange(GetStartupNotifiers());
            RunNotifiers(notifiers);
        }

        private void RunNotifiers(List<INotifier> notifiers)
        {
            notifiers.ForEach(x => x.Run().ForEach(y => model.AddNotification(y)));
        }

        private void Model_NotificationsUpdated(object? sender, EventArgs e)
        {
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            Notifications.Clear();
            Model.Model.Instance.Notifications.ForEach(x => Notifications.Add(x));
            OnPropertyChanged(nameof(Notifications));
        }
    }
}
