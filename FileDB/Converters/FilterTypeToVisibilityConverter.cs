using System;
using Avalonia.Data.Converters;
using FileDB.Model;

namespace FileDB.Converters;

public class FilterTypeToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return
            value is FilterType filterType &&
            parameter is FilterType viewType &&
            filterType == viewType;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
