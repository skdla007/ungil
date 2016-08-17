
namespace ArcGISControl.UIControl.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;

    public class VisibilityInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Inverse(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Inverse(value);
        }

        private Visibility Inverse(object value)
        {
            if (value is Visibility == false)
                throw new ArgumentException("value가 System.Windows.Visibility가 아닙니다.");

            if ((Visibility)value == Visibility.Visible)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }
    }
}
