using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ArcGISControl.Helper
{
    public static class ColorUtil
    {
        public static Color Transparentize(Color color)
        {
            return Color.FromArgb(0, color.R, color.G, color.B);
        }
    }
}
