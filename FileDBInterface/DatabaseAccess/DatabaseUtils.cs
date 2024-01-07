using System;

namespace FileDBInterface.DatabaseAccess;

public static class DatabaseUtils
{
    public static double CalculateDistance(double point1Lat, double point1Lon, double point2Lat, double point2Long)
    {
        var d1 = point1Lat * (Math.PI / 180.0);
        var num1 = point1Lon * (Math.PI / 180.0);
        var d2 = point2Lat * (Math.PI / 180.0);
        var num2 = point2Long * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                 Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

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
