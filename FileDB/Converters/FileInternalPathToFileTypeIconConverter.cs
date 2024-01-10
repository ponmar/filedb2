using FileDB.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class FileInternalPathToFileTypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string internalPath = (string)value;
        return FileTypeUtils.GetFileType(internalPath);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
