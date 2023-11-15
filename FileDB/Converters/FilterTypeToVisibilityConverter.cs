using FileDB.Model;
using System;
using System.Windows;
using System.Windows.Data;

namespace FileDB.Converters;

public class FilterTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var filterType = (FilterType)value;
        var viewType = (FilterType)parameter;
        return viewType == filterType ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
