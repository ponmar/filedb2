﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Configuration;

namespace FileDB.Converters;

public class SortMethodToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        var sortMethod = (SortMethod)value;
        return sortMethod.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
