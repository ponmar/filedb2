using System;

namespace FileDBAvalonia.Extensions;

public static class DateTimeExtensions
{
    public static string ToDateAndTime(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm");
    }

    public static string ToDate(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd");
    }
}
