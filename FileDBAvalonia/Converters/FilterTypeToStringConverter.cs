using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.Converters;

public class FilterTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentException();
        }
        var filterType = (FilterType)value;
        return filterType.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
