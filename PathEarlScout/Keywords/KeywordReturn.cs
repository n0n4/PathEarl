using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public struct KeywordReturn<T> where T : ITileInfo
    {
        public KeywordFloat<T> KeywordFloat;
        public KeywordInt<T> KeywordInt;
        public KeywordString<T> KeywordString;
    }
}
