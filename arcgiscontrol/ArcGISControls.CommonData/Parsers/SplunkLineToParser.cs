using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Parsers
{
    using System.Text.RegularExpressions;

    public class SplunkLineToParser
    {
        private static readonly Regex LineToPattern = new Regex(@"^\((.*),(.*)\),\((.*),(.*)\)$");

        public static Tuple<double, double, double, double> ParseLineInfo(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var match = LineToPattern.Match(data);
            if (!match.Success)
                return null;

            double lat1, long1, lat2, long2;

            if (!double.TryParse(match.Groups[1].Value, out lat1))
                return null;
            if (!double.TryParse(match.Groups[2].Value, out long1))
                return null;
            if (!double.TryParse(match.Groups[3].Value, out lat2))
                return null;
            if (!double.TryParse(match.Groups[4].Value, out long2))
                return null;

            return Tuple.Create(lat1, long1, lat2, long2);
        }
    }
}
