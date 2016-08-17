using System;
using System.Windows.Data;

namespace ArcGISControl.PropertyControl
{
    public class BooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    /// <summary>
    /// Common Property 창에 들어가는 Camera Property 창들의 BaseViewModel
    /// </summary>
    public abstract class CameraPropertyControlBaseViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private string cameraName;

        public string CameraName
        {
            get { return this.cameraName; }
            set
            {
                if (this.cameraName == value)
                    return;
                this.cameraName = value;
                this.OnPropertyChanged("CameraName");
            }
        }
    }
}
