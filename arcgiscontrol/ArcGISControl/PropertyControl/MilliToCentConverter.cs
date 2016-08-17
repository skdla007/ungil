
namespace ArcGISControl.PropertyControl
{
    using System;
    using System.Windows.Data;

    /// <summary>
    /// 0.1 단위(1000분의 1)를 10 단위로 변경한다.
    /// 0.1 => 10
    /// </summary>
    public class MilliToCentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value / 100;
        }
    }
}
