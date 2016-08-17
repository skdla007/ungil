using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcGISControl.Helper
{
    public class DoubleToStringConverterWithNaNNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var doubleValue = (double)value;
                double multiplier;
                if (double.TryParse((string)parameter, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out multiplier))
                {
                    doubleValue *= multiplier;
                }

                return double.IsNaN(doubleValue) ? null : doubleValue.ToString(culture);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (stringValue == null)
            {
                if (value != null)
                {
                    throw new ArgumentException("must provide string type value for 'value'", "value");
                }

                return double.NaN;
            }

            double parseResult;
            if (!double.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out parseResult))
            {
                return null;
            }

            double multiplier;
            if (double.TryParse((string)parameter, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out multiplier))
            {
                parseResult /= multiplier;
            }

            return parseResult;
        }
    }
}
