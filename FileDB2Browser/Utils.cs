using System;
using System.Windows;
using FileDB2Browser.Config;
using FileDB2Interface;
using FileDB2Interface.Exceptions;

namespace FileDB2Browser
{
    public static class Utils
    {
        public const string ApplicationTitle = "FileDB2";

        public static AppDataConfig<Config.Config> BrowserConfigIO { get; } = new(ApplicationTitle);

        public static Config.Config BrowserConfig { get; set; } = BrowserConfigIO.Read() ?? BrowserConfigFactory.GetDefault();

        public static IFileDB2Handle FileDB2Handle
        {
            get
            {
                if (fileDB2Handle == null)
                {
                    ReloadFileDB2Handle();
                }
                return fileDB2Handle;
            }
        }
        private static IFileDB2Handle fileDB2Handle;

        public static void ReloadFileDB2Handle()
        {
            var config = new FileDB2Config()
            {
                Database = BrowserConfig.Database,
                FilesRootDirectory = BrowserConfig.FilesRootDirectory,
            };
            try
            {
                fileDB2Handle = new FileDB2Handle(config);
            }
            catch (FileDB2Exception)
            {
                fileDB2Handle = new InvalidHandle();
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

        public static string GetBornYearsAgoString(DateTime now, string dateOfBirth)
        {
            if (InternalDatetimeToDatetime(dateOfBirth, out var result))
            {
                return GetBornYearsAgo(now, result.Value);
            }
            return string.Empty;
        }

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

        public static bool InternalDatetimeToDatetime(string datetimeStr, out DateTime? result)
        {
            if (datetimeStr != null && DateTime.TryParse(datetimeStr, out var datetime))
            {
                result = datetime;
                return true;
            }
            result = null;
            return false;
        }

        public static DateTime? InternalDatetimeToDatetime(string datetimeStr)
        {
            InternalDatetimeToDatetime(datetimeStr, out var result);
            return result;
        }

        public static void ShowInfoDialog(string message)
        {
            MessageBox.Show(message, ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
        }

        public static void ShowWarningDialog(string message)
        {
            MessageBox.Show(message, ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
        }

        public static bool ShowConfirmDialog(string question)
        {
            return MessageBox.Show(question, ApplicationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
