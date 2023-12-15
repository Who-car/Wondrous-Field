using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClientFrontend.UIElementHelpers;

public class ReversedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = (bool)value;
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visibilityValue = (Visibility)value;
        return visibilityValue != Visibility.Visible;
    }
}