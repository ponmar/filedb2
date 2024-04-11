using System;
using FileDBInterface.DatabaseAccess;

namespace FileDBInterface.Extensions;

public static class DateTimeExtensions
{
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
