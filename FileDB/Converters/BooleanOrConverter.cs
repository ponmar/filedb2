using System;
using System.Collections.Generic;
using Avalonia.Data.Converters;

namespace FileDB.Converters;

public class BooleanOrConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is bool boolValue && boolValue)
            {
                return true;
            }
        }
        return false;
    }
}
