using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FileDB.Config;
using FileDB.Notifiers;
using FileDB.Sorters;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;
using FluentValidation.Results;

namespace FileDB
{
    public class WindowModeDescription
    {
        public string Name => Mode.GetDescription();

        public WindowMode Mode { get; }

        public WindowModeDescription(WindowMode mode)
        {
            Mode = mode;
        }
    }

    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static Config.Config Config { get; set; }

        public static IDbAccess DbAccess
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
        private static IDbAccess dbAccess;

        public static void ReloadHandles()
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

        public static IFilesystemAccess FilesystemAccess
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
        private static IFilesystemAccess filesystemAccess;

        public static void ShowInfoDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
        }

        public static void ShowWarningDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
        }

        public static void ShowErrorDialog(IEnumerable<string> messages)
        {
            ShowErrorDialog(string.Join("\n", messages));
        }

        public static void ShowErrorDialog(ValidationResult validationResult)
        {
            ShowErrorDialog(validationResult.Errors.Select(x => x.ErrorMessage));
        }

        public static bool ShowConfirmDialog(string question)
        {
            return MessageBox.Show(question, ApplicationName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void ShowNotification(Notification notification)
        {
            switch (notification.Type)
            {
                case NotificationType.Info:
                    ShowInfoDialog(notification.Message);
                    break;
                case NotificationType.Warning:
                    ShowWarningDialog(notification.Message);
                    break;
                case NotificationType.Error:
                    ShowErrorDialog(notification.Message);
                    break;
            }
        }

        public static void ShowNotifications(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                ShowNotification(notification);
            }
        }

        public static void OpenUriInBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }

        public static string CreateFileList(List<FilesModel> files)
        {
            return files.Count > 0 ? string.Join(";", files.Select(x => x.Id)) : string.Empty;
        }

        public static List<SortMethodDescription> GetSortMethods()
        {
            List<SortMethodDescription> sortMethods = new();
            foreach (var value in Enum.GetValues<SortMethod>())
            {
                sortMethods.Add(new SortMethodDescription(value));
            }
            return sortMethods;
        }

        public static List<WindowModeDescription> GetWindowModes()
        {
            List<WindowModeDescription> modes = new();
            foreach (var mode in Enum.GetValues<WindowMode>())
            {
                modes.Add(new WindowModeDescription(mode));
            }
            return modes;
        }
    }
}
