using PathEarlCore;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public class KeywordFloat<T> : IPoolable where T : ITileInfo
    {
        public string KeywordOwner;
        public string Keyword;
        public Func<Tile<T>, float> Accessor;
        public float Literal;

        public bool HasNext = false;
        public string NextOperation;
        public KeywordString<T> NextString;
        public KeywordInt<T> NextInt;
        public KeywordFloat<T> NextFloat;

        private int PoolIndex;

        public override string ToString()
        {
            if (KeywordOwner != null)
                return KeywordOwner + "." + Keyword;
            return Keyword;
        }

        public void Clear()
        {
            KeywordOwner = null;
            Keyword = null;
            Accessor = null;
            Literal = 0;

            HasNext = false;
            NextOperation = null;
            NextString = null;
            NextInt = null;
            NextFloat = null;
        }

        public float Value(KeywordContext<T> context)
        {
            float value = InnerValue(context);
            if (!HasNext)
                return value;

            if (NextString != null)
            {
                string nextValueText = NextString.Value(context);
                if (float.TryParse(nextValueText, out float nextValue))
                    return KeywordHelper<T>.CombineFloats(value, nextValue, NextOperation);
                throw new Exception("Could not parse " + NextString.KeywordOwner + "." + NextString.Keyword + " value '" + nextValueText + "' as float");
            }
            else if (NextInt != null)
            {
                return KeywordHelper<T>.CombineFloats(value, NextInt.Value(context), NextOperation);
            }
            else if (NextFloat != null)
            {
                return KeywordHelper<T>.CombineFloats(value, NextFloat.Value(context), NextOperation);
            }
            throw new Exception("Expected next keyword after " + ToString());
        }

        private float InnerValue(KeywordContext<T> context)
        {
            if (Accessor != null)
            {
                Tile<T> tile = context.GetTile(KeywordOwner);
                return Accessor(tile);
            }
            return Literal;
        }

        public bool Compare(KeywordFloat<T> against, string operation, KeywordContext<T> context)
        {
            float a = Value(context);
            float b = against.Value(context);
            return KeywordHelper<T>.CompareFloats(a, b, operation);
        }

        public bool Compare(KeywordInt<T> against, string operation, KeywordContext<T> context)
        {
            float a = Value(context);
            float b = (float)against.Value(context);
            return KeywordHelper<T>.CompareFloats(a, b, operation);
        }

        public bool Compare(KeywordString<T> against, string operation, KeywordContext<T> context)
        {
            float a = Value(context);
            string bText = against.Value(context);
            if (float.TryParse(bText, out float b))
                return KeywordHelper<T>.CompareFloats(a, b, operation);
            return KeywordHelper<T>.CompareStrings(a.ToString(), bText, operation);
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
