using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Configuration;
using FileDB.Extensions;

namespace FileDB.Converters;

public class ThemeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        var theme = (Theme)value;
        return theme.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
