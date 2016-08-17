using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGISControl.Helper
{
    class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s != null)
            {
                var color = BrushUtil.ConvertFromString(s);
                return color;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                var brush = (Brush)value;
                return brush.ToString();
            }

            return null;
        }
    }
}
