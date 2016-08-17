using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace ArcGISControl.Helper
{
    public class RectComparer : IComparer<KeyValuePair<Screen, Rect>>
    {
        public int Compare(KeyValuePair<Screen, Rect> x, KeyValuePair<Screen, Rect> y)
        {
            var xArea = x.Value.Width * x.Value.Height;
            var yArea = y.Value.Width * y.Value.Height;

            //if (xArea == yArea)
            //{
            //    return 0;
            //}

            //if (xArea == 0)
            //{
            //    return -1;
            //}

            //if (yArea == 0)
            //{
            //    return 1;
            //}

            return xArea.CompareTo(yArea);
        }
    }
}
