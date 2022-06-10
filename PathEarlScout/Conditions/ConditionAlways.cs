using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionAlways<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "always";

        public ConditionAlways(ScoutRecycler<T> recycler)
        {
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
        }

        public bool Evaluate(KeywordContext<T> context)
        {
            return true;
        }

        public string GetKeyword()
        {
            return Keyword;
        }

        public void Load(ScoutSerializer<T> serializer)
        {
        }

        public void Save(ScoutSerializer<T> serializer)
        {
        }
    }
}
