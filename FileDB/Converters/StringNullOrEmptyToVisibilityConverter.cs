using FileDBInterface.Extensions;
using System;
using System.Windows;
using System.Windows.Data;

namespace FileDB.Converters;

public class StringNullOrEmptyToVisibilityConverter : System.Windows.Markup.MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (value as string).HasContent() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
