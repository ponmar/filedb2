using Avalonia.Data.Converters;
using FileDBAvalonia.Extensions;
using FileDBInterface.DatabaseAccess;
using System;
using System.Globalization;

namespace FileDBAvalonia.Converters;

public class SeasonToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentException();
        }

        var season = (Season)value;
        return season.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
