using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ArcGISControl.PropertyControl
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return CompareEnumToArgument(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }

        public static bool CompareEnumToArgument(object value, object parameter)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString());
        }
    }
}
