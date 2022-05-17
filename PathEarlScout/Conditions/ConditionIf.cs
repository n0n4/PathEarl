using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionIf<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "if";
        public ScoutCheck<T> Check;

        public ConditionIf(ScoutRecycler<T> recycler)
        {
            Check = recycler.ScoutCheckPool.Request();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            Check.Clear(recycler);
        }

        public bool Evaluate(KeywordContext<T> context)
        {
            return Check.Evaluate(context);
        }

        public string GetKeyword()
        {
            return Keyword;
        }

        public void Load(ScoutSerializer<T> serializer)
        {
            if (!serializer.TryReadLine())
                throw new Exception("Expected condition following 'if'");
            ScoutScript<T>.LoadCheck(serializer, serializer.LastLine, 0, Check);
        }

        public void Save(ScoutSerializer<T> serializer)
        {
            ScoutScript<T>.SaveCheck(serializer, Check);
            serializer.WriteLine("");
        }
    }
}
