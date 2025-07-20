using System.Globalization;
using System.Windows.Data;

namespace SmartEstimateApp.Converters;

public class PageNumberToIsActiveConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is int currentPage && values[1] is int pageNum)
            return currentPage == pageNum;
        return false;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
