using FileDB.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class FilterTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var filterType = (FilterType)value;
        return filterType.ToFriendlyString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
