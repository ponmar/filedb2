using Avalonia.Data.Converters;
using FileDB.Notifications;
using System;

namespace FileDB.Converters;

public class NotificationSeverityToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return (NotificationSeverity?)value switch
        {
            NotificationSeverity.Info => "\xD83D\xDEC8",
            NotificationSeverity.Warning or NotificationSeverity.Error => "\x26A0",
            _ => throw new NotSupportedException(),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
