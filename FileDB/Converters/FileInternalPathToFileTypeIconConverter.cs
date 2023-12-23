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
        var fileType = FileTypeUtils.GetFileType(internalPath);
        if (fileType is null)
        {
            return string.Empty;
        }

        return ((FileType)fileType).GetIcon();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
