namespace ArcGISControls.CommonData.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class SplunkMapListParser
    {
        /// <summary>
        /// Comma로 구분된 Map 이름 목록을 Parse해서 돌려준다
        /// RFC 4180 참고. CSV 형식인데 한 줄만 받는다.
        /// </summary>
        public static List<string> ParseMapList(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            var result = new List<String>();

            var isQuoteState = false;
            var field = new StringBuilder();

            const char quote = '\"';
            const char comma = ',';

            for (var i = 0; i < input.Length; i++)
            {
                if (isQuoteState)
                {
                    if (input[i] == quote)
                    {
                        // double quote escape
                        if (i + 1 < input.Length && input[i + 1] == quote)
                        {
                            i++;
                        }
                        else
                        {
                            isQuoteState = false;
                            continue;
                        }
                    }
                }
                else
                {
                    if (input[i] == comma)
                    {
                        result.Add(field.ToString());
                        field.Clear();
                        continue;
                    }
                    if (input[i] == quote)
                    {
                        isQuoteState = true;
                        continue;
                    }
                }
                field.Append(input[i]);
            }

            result.Add(field.ToString());
            return result;
        }
    }
}
