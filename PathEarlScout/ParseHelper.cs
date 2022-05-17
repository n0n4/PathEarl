using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public static class ParseHelper
    {
        public static string ReadToken(string body, char end, int pos)
        {
            if (pos >= body.Length)
                return string.Empty;

            int c = pos;
            while (c < body.Length)
            {
                if (body[c] == end)
                {
                    return body.Substring(pos, c - pos);
                }
                c++;
            }
            return body.Substring(pos);
        }

        public static string ReadTokenIgnoreQuotes(string body, char end, int pos)
        {
            if (pos >= body.Length)
                return string.Empty;

            bool inQuotes = false;
            int c = pos;
            while (c < body.Length)
            {
                if (body[c] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (body[c] == end && !inQuotes)
                {
                    return body.Substring(pos, c - pos);
                }
                c++;
            }
            return body.Substring(pos);
        }

        public static int SkipSpaces(string body, int pos)
        {
            while (pos < body.Length && body[pos] == ' ')
                pos++;
            return pos;
        }
    }
}
