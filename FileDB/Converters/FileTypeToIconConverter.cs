using Avalonia.Data.Converters;
using FileDB.Extensions;
using FileDBInterface.Model;
using System;
using System.Globalization;

namespace FileDB.Converters;

public class FileTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }
        var fileType = (FileType)value;
        return fileType.ToIcon();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
