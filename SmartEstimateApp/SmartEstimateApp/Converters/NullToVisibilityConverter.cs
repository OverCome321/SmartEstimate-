using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartEstimateApp.Converters;

/// <summary>
/// Преобразует null ⇄ Visibility:
///   без параметра: null → Collapsed, non-null → Visible
///   с ConverterParameter="Invert": null → Visible, non-null → Collapsed
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = (parameter as string)?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
        bool isNull = value == null;

        if (invert)
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        else
            return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}