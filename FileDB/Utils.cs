using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FileDB.Notifications;
using FileDBInterface.Access;
using FileDBInterface.Exceptions;
using FluentValidation.Results;

namespace FileDB
{
    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static Config.Config Config { get; set; }

        public static IDatabaseAccess DatabaseWrapper
        {
            get
            {
                if (databaseWrapper == null)
                {
                    ReloadFileDBHandle();
                }
                return databaseWrapper;
            }
        }
        private static IDatabaseAccess databaseWrapper;

        public static void ReloadFileDBHandle()
        {
            try
            {
                databaseWrapper = new DatabaseAccess(Config.Database, Config.FilesRootDirectory);
            }
            catch (DatabaseWrapperException)
            {
                databaseWrapper = new InvalidHandle();
            }
        }

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
                    Utils.ShowInfoDialog(notification.Message);
                    break;
                case NotificationType.Warning:
                    Utils.ShowWarningDialog(notification.Message);
                    break;
                case NotificationType.Error:
                    Utils.ShowErrorDialog(notification.Message);
                    break;
            }
        }

        public static void OpenUriInBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
    }
}
