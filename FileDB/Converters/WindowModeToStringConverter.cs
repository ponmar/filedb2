using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Configuration;

namespace FileDB.Converters;

public class WindowModeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }
        var windowMode = (WindowMode)value;
        return windowMode.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
