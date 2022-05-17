using PathEarlCore;
using PathEarlScout.Keywords;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class ScoutCheck<T> : IPoolable where T : ITileInfo
    {
        public KeywordFloat<T> FirstKeywordFloat;
        public KeywordInt<T> FirstKeywordInt;
        public KeywordString<T> FirstKeywordString;
        public string Operation;
        public KeywordFloat<T> SecondKeywordFloat;
        public KeywordInt<T> SecondKeywordInt;
        public KeywordString<T> SecondKeywordString;

        private int PoolIndex;

        public int GetPoolIndex()
        {
            return PoolIndex;
        }

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            Operation = null;

            if (FirstKeywordFloat != null)
            {
                recycler.KeywordFloatPool.Return(FirstKeywordFloat);
                FirstKeywordFloat = null;
            }
            if (FirstKeywordInt != null)
                recycler.KeywordIntPool.Return(FirstKeywordInt);
            if (FirstKeywordString != null)
                recycler.KeywordStringPool.Return(FirstKeywordString);
            if (SecondKeywordFloat != null)
                recycler.KeywordFloatPool.Return(SecondKeywordFloat);
            if (SecondKeywordInt != null)
                recycler.KeywordIntPool.Return(SecondKeywordInt);
            if (SecondKeywordString != null)
                recycler.KeywordStringPool.Return(SecondKeywordString);
        }

        public bool Evaluate(KeywordContext<T> context)
        {
            if (FirstKeywordFloat != null)
            {
                if (SecondKeywordFloat != null)
                    return FirstKeywordFloat.Compare(SecondKeywordFloat, Operation, context);
                if (SecondKeywordInt != null)
                    return FirstKeywordFloat.Compare(SecondKeywordInt, Operation, context);
                if (SecondKeywordString != null)
                    return FirstKeywordFloat.Compare(SecondKeywordString, Operation, context);
            }
            if (FirstKeywordInt != null)
            {
                if (SecondKeywordFloat != null)
                    return FirstKeywordInt.Compare(SecondKeywordFloat, Operation, context);
                if (SecondKeywordInt != null)
                    return FirstKeywordInt.Compare(SecondKeywordInt, Operation, context);
                if (SecondKeywordString != null)
                    return FirstKeywordInt.Compare(SecondKeywordString, Operation, context);
            }
            if (FirstKeywordString != null)
            {
                if (SecondKeywordFloat != null)
                    return FirstKeywordString.Compare(SecondKeywordFloat, Operation, context);
                if (SecondKeywordInt != null)
                    return FirstKeywordString.Compare(SecondKeywordInt, Operation, context);
                if (SecondKeywordString != null)
                    return FirstKeywordString.Compare(SecondKeywordString, Operation, context);
            }
            throw new Exception("Expected keywords in check");
        }
    }
}
