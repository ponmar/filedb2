using FileDB.Extensions;
using FileDB.Notifiers;
using FileDB.ViewModel;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace FileDB.Converters;

public class FileInternalPathToFileTypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var internalFilePath = (string)value;
        if (internalFilePath == string.Empty)
        {
            return string.Empty;
        }

        var fileExtension = Path.GetExtension(internalFilePath);
        if (fileExtension == null)
        {
            return string.Empty;
        }

        fileExtension = fileExtension.ToLower();
        var fileTypes = Enum.GetValues<FileType>().Where(x => x.GetAttribute<FileExtensionsAttribute>().FileExtensions.Contains(fileExtension)).ToList();
        if (!fileTypes.Any())
        {
            return string.Empty;
        }

        return fileTypes[0] switch
        {
            FileType.Picture => "\xD83D\xDDBC", // Unicode character name "Frame with Picture"
            FileType.Movie => "\xD83C\xDFAC", // Unicode character name "Clapper Board"
            FileType.Document => "\xD83D\xDDCE", // Unicode character name "Document"
            FileType.Audio => "\xD83C\xDFB5", // Unicode character name "Musical Note"
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
