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
        public KeywordFloat<T> KeywordFloat;
        public KeywordInt<T> KeywordInt;
        public KeywordString<T> KeywordString;
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
            if (KeywordInt != null)
            {
                recycler.KeywordIntPool.Return(KeywordInt);
                KeywordInt = null;
            }
            if (KeywordString != null)
            {
                recycler.KeywordStringPool.Return(KeywordString);
                KeywordString = null;
            }
            if (KeywordFloat != null)
            {
                recycler.KeywordFloatPool.Return(KeywordFloat);
                KeywordFloat = null;
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

        public string GetOwner(KeywordContext<T> context)
        {
            if (KeywordInt != null)
                return KeywordInt.KeywordOwner.Value(context);
            else if (KeywordFloat != null)
                return KeywordFloat.KeywordOwner.Value(context);
            else if (KeywordString != null)
                return KeywordString.KeywordOwner.Value(context);
            else 
                throw new Exception("Outcome lacks keyword");
        }

        public string GetKeyword(KeywordContext<T> context)
        {
            if (KeywordInt != null)
                return KeywordInt.Keyword.Value(context);
            else if (KeywordFloat != null)
                return KeywordFloat.Keyword.Value(context);
            else if (KeywordString != null)
                return KeywordString.Keyword.Value(context);
            else
                throw new Exception("Outcome lacks keyword");
        }

        public float GetProbability(KeywordContext<T> context)
        {
            float prob = 1;
            if (Probability != null)
                prob = Probability.Value(context);
            return prob;
        }
    }
}
