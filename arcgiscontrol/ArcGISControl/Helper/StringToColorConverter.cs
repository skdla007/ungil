using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGISControl.Helper
{
    public class StringToColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = value as string;
            if (s != null)
            {
                var color = ColorConverter.ConvertFromString(s);
                return color;
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                var color = (Color)value;
                return color.ToString();
            }

            return null;
        }
    }
}
