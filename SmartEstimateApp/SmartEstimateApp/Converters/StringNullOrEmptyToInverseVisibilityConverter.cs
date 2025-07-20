using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartEstimateApp.Converters;

public class StringNullOrEmptyToInverseVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value as string;
        return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
