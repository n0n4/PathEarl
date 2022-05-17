using PathEarlCore;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public class KeywordString<T> : IPoolable where T : ITileInfo
    {
        public string KeywordOwner;
        public string Keyword;
        public Func<T, string> Accessor;
        public string Literal;

        public bool HasNext = false;
        public string NextOperation;
        public KeywordString<T> NextString;
        public KeywordInt<T> NextInt;
        public KeywordFloat<T> NextFloat;

        private int PoolIndex;

        public void Clear()
        {
            KeywordOwner = null;
            Keyword = null;
            Accessor = null;
            Literal = null;

            HasNext = false;
            NextOperation = null;
            NextString = null;
            NextInt = null;
            NextFloat = null;
        }

        public string Value(KeywordContext<T> context)
        {
            string value = InnerValue(context);
            if (!HasNext)
                return value;

            if (NextString != null)
            {
                return KeywordHelper<T>.CombineStrings(value, NextString.Value(context), NextOperation);
            }
            else if (NextInt != null)
            {
                return KeywordHelper<T>.CombineStrings(value, NextInt.Value(context).ToString(), NextOperation);
            }
            else if (NextFloat != null)
            {
                return KeywordHelper<T>.CombineStrings(value, NextFloat.Value(context).ToString(), NextOperation);
            }
            throw new Exception("Expected next keyword after " + KeywordOwner + "." + Keyword);
        }

        public string InnerValue(KeywordContext<T> context)
        {
            if (Accessor != null)
            {
                T info = context.GetInfo(KeywordOwner);
                return Accessor(info);
            }
            return Literal;
        }

        public bool Compare(KeywordString<T> against, string operation, KeywordContext<T> context)
        {
            string a = Value(context);
            string b = against.Value(context);
            return KeywordHelper<T>.CompareStrings(a, b, operation);
        }

        public bool Compare(KeywordFloat<T> against, string operation, KeywordContext<T> context)
        {
            float a = against.Value(context);
            string bText = Value(context);
            if (float.TryParse(bText, out float b))
                return KeywordHelper<T>.CompareFloats(a, b, operation);
            return KeywordHelper<T>.CompareStrings(a.ToString(), bText, operation);
        }

        public bool Compare(KeywordInt<T> against, string operation, KeywordContext<T> context)
        {
            int a = against.Value(context);
            string bText = Value(context);
            if (int.TryParse(bText, out int b))
                return KeywordHelper<T>.CompareInts(a, b, operation);
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
