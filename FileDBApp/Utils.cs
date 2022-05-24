using System.Globalization;

namespace FileDBApp
{
    public static class Utils
    {
        public static DateTime ParsePersonsDateOfBirth(string dateOfBirthStr)
        {
            return DateTime.ParseExact(dateOfBirthStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static int GetDaysToNextBirthday(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
            {
                next = next.AddYears(1);
            }

            return (next - today).Days;
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
    }
}
