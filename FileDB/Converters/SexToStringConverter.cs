using Avalonia.Data.Converters;
using FileDB.Extensions;
using FileDBInterface.Model;
using System;
using System.Globalization;

namespace FileDB.Converters;

public class SexToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentException();
        }

        var sex = (Sex)value;
        return sex.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
