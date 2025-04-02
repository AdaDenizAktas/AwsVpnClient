using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace AwsVpnClient.UI.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter() { }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString() == "Invert";
            bool visible = (bool)value;
            return (invert ? !visible : visible) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool invert = parameter?.ToString() == "Invert";
                bool result = visibility == Visibility.Visible;
                return invert ? !result : result;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
