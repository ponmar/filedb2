﻿using FileDB.Configuration;
using FileDB.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FileDB.Converters;

public class WindowModeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var windowMode = (WindowMode)value;
        return windowMode.ToFriendlyString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
