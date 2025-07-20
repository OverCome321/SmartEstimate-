using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartEstimateApp.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public Brush ActiveBrush { get; set; } = new SolidColorBrush(Color.FromRgb(52, 199, 89)); // AppleSuccess
    public Brush InactiveBrush { get; set; } = new SolidColorBrush(Color.FromRgb(134, 134, 139)); // AppleTextSecondary
    public Brush BlockedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(255, 59, 48)); // AppleError

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int status = value is int i ? i : 0;
        return status switch
        {
            1 => ActiveBrush,   // Активен
            2 => BlockedBrush,  // Заблокирован
            _ => InactiveBrush, // Неактивен/по умолчанию
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
