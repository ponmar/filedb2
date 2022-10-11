using System;
using System.Windows.Data;

namespace FileDB.Converters;

public class IntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((int)value).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (int.TryParse((string)value, out var intValue))
        {
            return intValue;
        }
        return 0;
    }
}
