using FileDBInterface.Extensions;
using System;
using System.Windows.Data;

namespace FileDB.Converters;

public class StringContentToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (value as string).HasContent();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
