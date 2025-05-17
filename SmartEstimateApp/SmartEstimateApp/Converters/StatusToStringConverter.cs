using System.Globalization;
using System.Windows.Data;

namespace SmartEstimateApp.Converters;

public class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int status = value is int i ? i : 0;
        return status switch
        {
            1 => "Активен",
            2 => "Неактивен",
            _ => "Неактивен"
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
