using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ArcGISControl.Helper
{
    public class BoolToFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(targetType == typeof(FontWeight))
            {
                if (value is bool?)
                {
                    var v = (bool?)value;
                    return v.Value == true ? FontWeights.Bold : FontWeights.Normal;
                }

                return FontWeights.Normal;
            }

            if (targetType == typeof(FontStyle))
            {
                if (value is bool?)
                {
                    var v = (bool?)value;
                    return v.Value == true ? FontStyles.Italic : FontStyles.Normal;
                }

                return FontStyles.Normal;
            }

            if (targetType == typeof(TextDecorationCollection))
            {
                if (value is bool?)
                {
                    var v = (bool?)value;
                    return v.Value == true ? TextDecorations.Underline : null;
                }

                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
