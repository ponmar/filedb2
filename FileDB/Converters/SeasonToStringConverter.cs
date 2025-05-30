﻿using Avalonia.Data.Converters;
using FileDB.Extensions;
using FileDBInterface.DatabaseAccess;
using System;
using System.Globalization;

namespace FileDB.Converters;

public class SeasonToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        var season = (Season)value;
        return season.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
