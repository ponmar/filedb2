using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileDB.Model;

namespace FileDB.Converters;

public class FileInternalPathToFileTypeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }
        var internalPath = (string)value;
        return FileTypeUtils.GetFileType(internalPath);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
