using FileDB.Extensions;
using FileDB.ViewModel;
using FileDBShared.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class SexToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var sex = (Sex)value;
        return sex.ToFriendlyString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
