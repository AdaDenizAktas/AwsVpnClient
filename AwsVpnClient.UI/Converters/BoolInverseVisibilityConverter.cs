using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AwsVpnClient.UI.Converters
{
    public class BoolInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility != Visibility.Visible;

            return DependencyProperty.UnsetValue;
        }
    }
}
