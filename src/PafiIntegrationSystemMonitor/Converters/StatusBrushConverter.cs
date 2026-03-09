using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.Converters;

public sealed class StatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MonitorStatus status) return Brushes.Gray;

        return status switch
        {
            MonitorStatus.Healthy => new SolidColorBrush(Color.FromRgb(38, 166, 91)),
            MonitorStatus.Warning => new SolidColorBrush(Color.FromRgb(242, 153, 74)),
            MonitorStatus.Failed => new SolidColorBrush(Color.FromRgb(235, 87, 87)),
            _ => new SolidColorBrush(Color.FromRgb(130, 130, 130)),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
