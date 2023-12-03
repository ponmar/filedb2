using FileDB.Configuration;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class SortMethodToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var sortMethod = (SortMethod)value;
        return sortMethod.ToFriendlyString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
