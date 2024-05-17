﻿using Avalonia.Data.Converters;
using FileDBAvalonia.Extensions;
using FileDBInterface.FileFormats;
using System;
using System.Globalization;

namespace FileDBAvalonia.Converters;

public class FileTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentException();
        }
        var fileType = (FileType)value;
        return fileType.ToFriendlyString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
