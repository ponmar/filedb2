using FileDB.ViewModel;
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
        if (fileType == null)
        {
            return string.Empty;
        }

        return fileType switch
        {
            FileType.Picture => "\xD83D\xDDBC", // Unicode character name "Frame with Picture"
            FileType.Movie => "\xD83C\xDFAC", // Unicode character name "Clapper Board"
            FileType.Document => "\xD83D\xDDCE", // Unicode character name "Document"
            FileType.Audio => "\xD83C\xDFB5", // Unicode character name "Musical Note"
            _ => throw new NotImplementedException(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
