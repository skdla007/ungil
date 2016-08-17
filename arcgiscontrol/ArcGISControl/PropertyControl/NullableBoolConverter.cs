using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ArcGISControl.PropertyControl
{
    public class NullableBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,CultureInfo culture)
        {
            bool? result = null;

            if (!string.IsNullOrEmpty(value as string))
            {
                bool parsedResult = false;

                if (bool.TryParse(value as string, out parsedResult)) result = parsedResult;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter,CultureInfo culture)
        {
            return value;
        }
    }
}
