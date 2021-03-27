using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Browser.Config;
using FileDB2Interface;

namespace FileDB2Browser
{
    public static class Utils
    {
        public static FileDB2BrowserConfig BrowserConfig { get; } = FileDB2BrowserConfigIO.Read();

        public static FileDB2Handle FileDB2Handle
        {
            get
            {
                if (fileDB2Handle == null)
                {
                    var config = new FileDB2Config()
                    {
                        Database = BrowserConfig.Database,
                        FilesRootDirectory = BrowserConfig.FilesRootDirectory,
                    };
                    fileDB2Handle = new FileDB2Handle(config);
                }
                return fileDB2Handle;
            }
        }
        private static FileDB2Handle fileDB2Handle;

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
    }
}
