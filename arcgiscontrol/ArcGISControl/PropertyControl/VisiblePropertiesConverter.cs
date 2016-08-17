using System;
using System.Linq;
using System.Windows.Data;

namespace ArcGISControl.PropertyControl
{
    class VisiblePropertiesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MapObjectPropertied mapObjectPropertied = (MapObjectPropertied)Enum.Parse(typeof(MapObjectPropertied), parameter.ToString());
            if (((MapObjectPropertied[])value).Contains(mapObjectPropertied))
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
