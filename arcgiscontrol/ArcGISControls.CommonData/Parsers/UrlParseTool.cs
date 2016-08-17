using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Parsers
{
    public class UrlParseTool
    {
        // Per RFC 3986
        public static string ExtractScheme(string url)
        {
            var colonIndex = url.IndexOf(':');
            if (colonIndex == -1)
            {
                return null;
            }

            if (!IsAlpha(url.First()))
            {
                return null;
            }

            if (url.Take(colonIndex).All(ch => IsAlpha(ch) || IsDigit(ch) || ch == '+' || ch == '-' || ch == '.'))
            {
                return url.Substring(0, colonIndex);
            }

            return null;
        }

        // Per RFC 2234
        protected static bool IsAlpha(char ch)
        {
            return ('\x41' <= ch && ch <= '\x5A') ||
                   ('\x61' <= ch && ch <= '\x7A');
        }

        protected static bool IsDigit(char ch)
        {
            return '\x30' <= ch && ch <= '\x39';
        }
    }
}
