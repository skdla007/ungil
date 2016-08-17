using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ArcGISControls.Tools.PostItControl
{
    public class BoolToVisibleDebuggingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Input : {0}", value));
            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            InnowatchDebug.Logger.Trace("구현 안됨");
            throw new NotImplementedException();
        }
    }
}
