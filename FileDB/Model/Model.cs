using System;
using System.Collections.Generic;
using FileDB.Notifiers;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.FilesystemAccess;

namespace FileDB.Model
{
    public class Model
    {
        public static Model Instance => instance ??= new();
        private static Model instance;

        public List<Notification> Notifications { get; } = new();

        public event EventHandler NotificationsUpdated;

        public void AddNotification(Notification notification)
        {
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

        public Config.Config Config { get; set; }

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
    }
}
