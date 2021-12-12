using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace FileDB.Converters
{
    public class EnumerationHasContentConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null && ((IEnumerable)value).GetEnumerator().MoveNext();
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
}
