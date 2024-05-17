using System;

namespace FileDBInterface.Utils;

public static class TimeUtils
{
    public static int GetAgeInYears(DateTime end, DateTime start)
    {
        return Math.Max(0, GetYearsAgo(end, start));
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
}
