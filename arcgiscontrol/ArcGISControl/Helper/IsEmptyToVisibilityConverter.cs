using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArcGISControl.Helper
{
    public class IsEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            return string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InnowatchDebug.Logger.Trace("구현 안됨");
            throw new NotImplementedException();
        }
    }
}
