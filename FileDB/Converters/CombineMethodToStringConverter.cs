using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Model;
using FileDB.ViewModels.Search;

namespace FileDB.Converters;

public class CombineMethodToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        var combineMethod = (CombineMethod)value;
        return combineMethod.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
