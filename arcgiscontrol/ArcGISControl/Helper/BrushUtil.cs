using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArcGISControl.Helper
{
    public static class BrushUtil
    {
        private static readonly BrushConverter converter = new BrushConverter();

        public static SolidColorBrush ConvertFromString(string colorString)
        {
            if (String.IsNullOrWhiteSpace(colorString)) return null;
            try { return converter.ConvertFromString(colorString) as SolidColorBrush; }
            catch { return null; }
        }

        public static string ConvertFromBrush(SolidColorBrush brush)
        {
            if (brush == null) return string.Empty;
            try { return converter.ConvertToString(brush); }
            catch { return string.Empty; }
        }

        public static ImageBrush CreateImageBrush(string imageUri)
        {
            try
            {
                return new ImageBrush(new BitmapImage(new Uri(imageUri)));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Brush SetBrightness(Brush original, double brightness)
        {
            Brush result = original;

            if (result is SolidColorBrush)
            {
                var hsb = HSBColor.FromColor(((SolidColorBrush) original).Color);
                hsb.B *= brightness;
                result = new SolidColorBrush(hsb.ToColor());
            }

            return result;
        }

        public static Brush SetSaturation(Brush original, double factor)
        {
            Brush result = original;

            if (result is SolidColorBrush)
            {
                var hsb = HSBColor.FromColor(((SolidColorBrush)original).Color);
                if (hsb.S == 0) hsb.B = 0.15;
                else hsb.S *= factor;
                result = new SolidColorBrush(hsb.ToColor());
            }

            return result;
        }
    }
}
