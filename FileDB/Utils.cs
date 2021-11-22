using System;
using System.Diagnostics;
using System.Windows;
using FileDB.Config;
using FileDBInterface;
using FileDBInterface.Exceptions;

namespace FileDB
{
    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static AppDataConfig<Config.Config> BrowserConfigIO { get; } = new(ApplicationName);

        public static Config.Config BrowserConfig { get; set; }

        public static IDatabaseWrapper FileDBHandle
        {
            get
            {
                if (fileDBHandle == null)
                {
                    ReloadFileDBHandle();
                }
                return fileDBHandle;
            }
        }
        private static IDatabaseWrapper fileDBHandle;

        public static void ReloadFileDBHandle()
        {
            try
            {
                fileDBHandle = new DatabaseWrapper(BrowserConfig.Database, BrowserConfig.FilesRootDirectory);
            }
            catch (DatabaseWrapperException)
            {
                fileDBHandle = new InvalidHandle();
            }
        }

        public static int GetYearsAgo(DateTime now, DateTime dateTime)
        {
            int yearsAgo = now.Year - dateTime.Year;

            try
            {
                if (new DateTime(dateTime.Year, now.Month, now.Day) < dateTime)
                {
                    yearsAgo--;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Current date did not exist the year that person was born
            }

            return yearsAgo;
        }

        /*
        public static string GetBornYearsAgoString(DateTime now, string dateOfBirth)
        {
            if (DatabaseParsing.InternalDatetimeToDatetime(dateOfBirth, out var result))
            {
                return GetBornYearsAgo(now, result.Value);
            }
            return string.Empty;
        }
        */

        public static string GetBornYearsAgo(DateTime now, DateTime dateOfBirth)
        {
            return GetYearsAgo(now, dateOfBirth).ToString();
        }

        public static int GetDaysToNextBirthday(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
                next = next.AddYears(1);

            return (next - today).Days;
        }

        public static int GetAgeInYears(DateTime dateOfBirth, DateTime deceased)
        {
            return GetYearsAgo(deceased, dateOfBirth);
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
