using PathEarlCore;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public class KeywordInt<T> : IPoolable where T : ITileInfo
    {
        public KeywordString<T> KeywordOwner;
        public KeywordString<T> Keyword;
        public Func<Tile<T>, int> Accessor;
        public int Literal;

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
            Literal = 0;

            HasNext = false;
            NextOperation = null;
            NextString = null;
            NextInt = null;
            NextFloat = null;
        }

        public int Value(KeywordContext<T> context)
        {
            int value = InnerValue(context);
            if (!HasNext)
                return value;

            if (NextString != null)
            {
                string nextValueText = NextString.Value(context);
                if (int.TryParse(nextValueText, out int nextValue))
                    return KeywordHelper<T>.CombineInts(value, nextValue, NextOperation);
                throw new Exception("Could not parse " + NextString.KeywordOwner + "." + NextString.Keyword + " value '" + nextValueText + "' as int");
            }
            else if (NextInt != null)
            {
                return KeywordHelper<T>.CombineInts(value, NextInt.Value(context), NextOperation);
            }
            else if (NextFloat != null)
            {
                return KeywordHelper<T>.CombineInts(value, (int)NextFloat.Value(context), NextOperation);
            }
            throw new Exception("Expected next keyword after " + KeywordOwner + "." + Keyword);
        }

        public int InnerValue(KeywordContext<T> context)
        {
            if (Accessor != null)
            {
                Tile<T> tile = context.GetTile(KeywordOwner.Value(context));
                return Accessor(tile);
            } 
            else if (KeywordOwner != null) 
            {
                string owner = KeywordOwner.Value(context);
                string keyword = Keyword.Value(context);
                if (!context.InfoAccess.TryGetIntGet(owner, keyword, out Func<Tile<T>, int> func))
                    throw new Exception("Dynamically generated keyword '" + keyword + "' not found");
                Tile<T> tile = context.GetTile(owner);
                return func(tile);
            }
            return Literal;
        }

        public bool Compare(KeywordInt<T> against, string operation, KeywordContext<T> context)
        {
            int a = Value(context);
            int b = against.Value(context);
            return KeywordHelper<T>.CompareInts(a, b, operation);
        }

        public bool Compare(KeywordFloat<T> against, string operation, KeywordContext<T> context)
        {
            float a = (float)Value(context);
            float b = against.Value(context);
            return KeywordHelper<T>.CompareFloats(a, b, operation);
        }

        public bool Compare(KeywordString<T> against, string operation, KeywordContext<T> context)
        {
            int a = Value(context);
            string bText = against.Value(context);
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
