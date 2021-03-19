using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser
{
    public static class Utils
    {
        public static string GetBornYearsAgo(string dateOfBirth)
        {
            if (dateOfBirth != null && DateTime.TryParse(dateOfBirth, out var result))
            {
                return GetBornYearsAgo(result);
            }
            return string.Empty;
        }

        public static string GetBornYearsAgo(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int yearsAgo = now.Year - dateOfBirth.Year;

            try
            {
                if (new DateTime(dateOfBirth.Year, now.Month, now.Day) < dateOfBirth)
                {
                    yearsAgo--;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Current date did not exist the year that person was born
            }

            return yearsAgo.ToString();
        }

        public static int GetDaysToNextBirthday(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
                next = next.AddYears(1);

            return (next - today).Days;
        }
    }
}
