using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FileDBInterface;
using FileDBInterface.Exceptions;
using FluentValidation.Results;

namespace FileDB
{
    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static Config.Config Config { get; set; }

        public static IDatabaseWrapper DatabaseWrapper
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
        private static IDatabaseWrapper databaseWrapper;

        public static void ReloadFileDBHandle()
        {
            try
            {
                databaseWrapper = new DatabaseWrapper(Config.Database, Config.FilesRootDirectory);
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

        public static void OpenUriInBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
    }
}
