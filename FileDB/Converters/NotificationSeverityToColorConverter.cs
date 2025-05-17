using Avalonia.Data.Converters;
using Avalonia.Media;
using FileDB.Notifications;
using System;

namespace FileDB.Converters;

public class NotificationSeverityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return (NotificationSeverity?)value switch
        {
            NotificationSeverity.Info => new SolidColorBrush(Color.FromRgb(0, 127, 0)),
            NotificationSeverity.Warning => new SolidColorBrush(Color.FromRgb(255, 127, 0)),
            NotificationSeverity.Error => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            _ => throw new NotSupportedException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
