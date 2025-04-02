using System;
using System.Globalization;
using System.Windows.Data;

namespace AwsVpnClient.UI.Converters
{
    public class RegionEnableMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string activeRegion = values[0]?.ToString();
            string currentRegion = values[1]?.ToString();
            return !string.Equals(activeRegion, currentRegion, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class RegionDisableMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string activeRegion = values[0]?.ToString();
            string currentRegion = values[1]?.ToString();
            return string.Equals(activeRegion, currentRegion, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
