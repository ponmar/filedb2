using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Extensions;

namespace FileDBAvalonia.Converters;

public class ThemeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }
        var theme = (Theme)value;
        return theme.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
