using PathEarlCore;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public class ParsedKeyword<T> : IPoolable where T : ITileInfo
    {
        public KeywordFloat<T> Float;
        public KeywordInt<T> Int;
        public KeywordString<T> String;
        public string Error;

        private int PoolIndex;

        public void Clear()
        {
            Float = null;
            Int = null;
            String = null;
            Error = null;
        }

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }

        public int GetPoolIndex()
        {
            return PoolIndex;
        }
    }
}
