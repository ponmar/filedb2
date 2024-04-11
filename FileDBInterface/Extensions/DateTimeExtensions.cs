using System;
using FileDBInterface.DatabaseAccess;

namespace FileDBInterface.Extensions;

public static class DateTimeExtensions
{
    public static bool IsMonthAndDayInRange(this DateTime dateTime, int startMonth, int startDay, int endMonth, int endDay)
    {
        var start = new DateTime(dateTime.Year, startMonth, Math.Min(startDay, DateTime.DaysInMonth(dateTime.Year, startMonth)));
        var end = new DateTime(dateTime.Year, endMonth, Math.Min(endDay, DateTime.DaysInMonth(dateTime.Year, endMonth)));
        return dateTime >= start && dateTime <= end;
    }

    // Season start and end values are based on the 2024 season dates
    public static Season GetApproximatedSeason(this DateTime date)
    {
        var value = date.Month + date.Day / 100f;
        if (value < 3.185 || value >= 12.205)
        {
            return Season.Winter;
        }
        if (value < 6.195)
        {
            return Season.Spring;
        }
        if (value < 9.215)
        {
            return Season.Summer;
        }
        return Season.Autumn;
    }
}
