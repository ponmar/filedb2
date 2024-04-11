using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FileDBAvalonia.Converters;

public class NumberToMonthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentException();
        }
        var month = (int)value;
        return new DateTime(2024, month, 1).ToString("MMM");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
