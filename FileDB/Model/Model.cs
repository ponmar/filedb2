using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FileDB.Configuration;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;

namespace FileDB.Model
{
    public class Model
    {
        public static Model Instance => instance ??= new();
        private static Model instance;

        private Model()
        {
            var dateCheckerTimer = new DispatcherTimer();
            dateCheckerTimer.Interval = TimeSpan.FromMinutes(1);
            dateCheckerTimer.Tick += DateCheckerTimer_Tick;
            dateCheckerTimer.Start();
        }

        private DateTime date = DateTime.Now;
        public event EventHandler DateChanged;

        private void DateCheckerTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (date.Date != now.Date)
            {
                date = now;
                DateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<Notification> Notifications { get; } = new();

        public event EventHandler NotificationsUpdated;

        public void AddNotification(Notification notification)
        {
            Notifications.RemoveAll(x => x.Message == notification.Message);
            Notifications.Add(notification);
            NotificationsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void AddNotification(NotificationType type, string message)
        {
            AddNotification(new Notification(type, message, DateTime.Now));
        }

        public void ClearNotifications()
        {
            if (Notifications.Count > 0)
            {
                Notifications.Clear();
                NotificationsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ConfigLoaded;
        public Config Config
        {
            get => config;
            set
            {
                if (config != value)
                {
                    config = value;
                    ConfigLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private Config config;

        public IDbAccess DbAccess
        {
            get
            {
                if (dbAccess == null)
                {
                    ReloadHandles();
                }
                return dbAccess;
            }
        }
        private IDbAccess dbAccess;

        public void ReloadHandles()
        {
            try
            {
                dbAccess = new DbAccess(Config.Database);
            }
            catch (DatabaseWrapperException)
            {
                dbAccess = new NoDbAccess();
            }

            filesystemAccess = new FilesystemAccess(Config.FilesRootDirectory);
        }

        public IFilesystemAccess FilesystemAccess
        {
            get
            {
                if (filesystemAccess == null)
                {
                    ReloadHandles();
                }
                return filesystemAccess;
            }
        }
        private IFilesystemAccess filesystemAccess;

        public event EventHandler PersonsUpdated;
        public event EventHandler LocationsUpdated;
        public event EventHandler TagsUpdated;

        public void NotifyPersonsUpdated()
        {
            PersonsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyLocationsUpdated()
        {
            LocationsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyTagsUpdated()
        {
            TagsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<bool> TemporaryFullscreenRequested;

        public void RequestTemporaryFullscreen(bool fullscreen)
        {
            TemporaryFullscreenRequested?.Invoke(this, fullscreen);
        }

        public event EventHandler<List<FilesModel>> FilesImported;

        public void NotifyFilesImported(List<FilesModel> files)
        {
            FilesImported?.Invoke(this, files);
        }

        public void CastFile(string filePath)
        {
            if (!FileCaster.IsRunning() &&
                config.CastHttpServerPort > 0)
            {
                FileCaster.RunServer(config.CastHttpServerPort);
            }

            {
                FileCaster.CastFile(filePath);
            }
        }
    }
}
