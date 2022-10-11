using FileDB.Notifiers;
using System;
using System.Windows.Data;

namespace FileDB.Converters;

public class NotificationTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (NotificationType)value switch
        {
            NotificationType.Info => "\xD83D\xDEC8",
            NotificationType.Warning or NotificationType.Error => "\x26A0",
            _ => throw new NotSupportedException(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
