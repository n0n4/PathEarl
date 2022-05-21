using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class Outcome<T> where T : ITileInfo
    {
        public KeywordFloat<T> Probability;
        public KeywordString<T> Keyword;
        public string Operation;
        public KeywordFloat<T> ValueFloat;
        public KeywordInt<T> ValueInt;
        public KeywordString<T> ValueString;

        public Outcome(ScoutRecycler<T> recycler)
        {

        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            if (Probability != null)
            {
                recycler.KeywordFloatPool.Return(Probability);
                Probability = null;
            }
            if (Keyword != null)
            {
                recycler.KeywordStringPool.Return(Keyword);
                Keyword = null;
            }
            Operation = null;
            if (ValueFloat != null)
            {
                recycler.KeywordFloatPool.Return(ValueFloat);
                ValueFloat = null;
            }
            if (ValueInt != null)
            {
                recycler.KeywordIntPool.Return(ValueInt);
                ValueInt = null;
            }
            if (ValueString != null)
            {
                recycler.KeywordStringPool.Return(ValueString);
                ValueString = null;
            }
        }
    }
}
