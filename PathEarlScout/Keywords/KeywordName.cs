using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public struct KeywordName
    {
        public string Owner;
        public string Keyword;

        public KeywordName(string owner, string keyword)
        {
            Owner = owner;
            Keyword = keyword;
        }
    }
}
