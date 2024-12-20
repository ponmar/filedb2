﻿using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FileDB.Converters;

public class NumberToMonthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        var month = (int)value;
        return new DateTime(2024, month, 1).ToString("MMM");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
