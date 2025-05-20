using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartEstimateApp.Converters;

public class TableViewVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Visibility.Collapsed;
        bool isTable = values[0] is bool b1 && b1;
        bool hasResults = values[1] is bool b2 && b2;
        return (isTable && hasResults) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
