using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDBAvalonia.Configuration;

namespace FileDBAvalonia.Converters;

public class SortMethodToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }
        var sortMethod = (SortMethod)value;
        return sortMethod.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
