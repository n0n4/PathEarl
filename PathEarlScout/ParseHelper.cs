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

        public static string ReadTokenIgnoreBrackets(string body, char end, int pos,
            char open = '<', char close = '>')
        {
            if (pos >= body.Length)
                return string.Empty;

            bool inQuotes = false;
            int brackets = 0;
            int i = pos;
            while (i < body.Length)
            {
                char c = body[i];
                if (inQuotes)
                {
                    if (c == '"')
                        inQuotes = false;
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == open)
                {
                    brackets++;
                }
                else if (c == close)
                {
                    brackets--;
                }
                else if (brackets <= 0 && c == end)
                {
                    return body.Substring(pos, i - pos);
                }
                i++;
            }
            return body.Substring(pos);
        }

        public static int IndexOfIgnoreBrackets(string body, int pos, char target,
            char open = '<', char close = '>')
        {
            if (pos >= body.Length)
                return -1;

            int brackets = 0;
            bool inQuotes = false;
            while (pos < body.Length)
            {
                char c = body[pos];
                if (inQuotes)
                {
                    if (c == '"')
                        inQuotes = false;
                }
                else if (c == '"') 
                {
                    inQuotes = true;
                }
                else if (c == open)
                {
                    brackets++;
                }
                else if (c == close)
                {
                    brackets--;
                }
                else if (brackets <= 0 && c == target)
                {
                    return pos;
                }
                pos++;
            }
            return -1;
        }

        public static int SkipSpaces(string body, int pos)
        {
            while (pos < body.Length && body[pos] == ' ')
                pos++;
            return pos;
        }

        public static int FindLastNonSpace(string body)
        {
            int pos = body.Length - 1;
            while (pos > 0 && body[pos] == ' ')
                pos--;
            return pos;
        }
    }
}
