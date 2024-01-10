using FileDB.Extensions;
using FileDB.Model;
using FileDBShared.FileFormats;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class FileTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var fileType = (FileType)value;
        return fileType.ToFriendlyString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
