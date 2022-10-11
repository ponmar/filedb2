using FileDB.Notifiers;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace FileDB.Converters;

public class NotificationTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (NotificationType)value switch
        {
            NotificationType.Info => new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            NotificationType.Warning => new SolidColorBrush(Color.FromRgb(255, 165, 0)),
            NotificationType.Error => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            _ => throw new NotSupportedException(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
