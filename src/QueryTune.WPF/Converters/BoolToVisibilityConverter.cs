using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QueryTune.WPF.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool inverse = parameter?.ToString()?.ToLower() == "inverse";
                bool visible = inverse ? !boolValue : boolValue;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
