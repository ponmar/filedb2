using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Model;

namespace FileDB.Converters;

public class FilterTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        var filterType = (FilterType)value;
        return filterType.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
