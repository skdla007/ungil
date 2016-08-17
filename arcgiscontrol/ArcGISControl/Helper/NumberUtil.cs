using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.Helper
{
    public static class NumberUtil
    {
        public static bool AreSame(double lhs, double rhs)
        {
            const double tolerance = 1E-8;

            return Math.Abs((lhs - rhs) / lhs) < tolerance
                || Math.Abs(lhs - rhs) < tolerance;
        }
    }
}
