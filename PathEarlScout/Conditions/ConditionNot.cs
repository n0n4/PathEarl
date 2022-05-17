using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionNot<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "not";
        public List<ICondition<T>> Conditions;

        public ConditionNot(ScoutRecycler<T> recycler)
        {
            Conditions = recycler.GetConditionList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach (ICondition<T> cond in Conditions)
            {
                recycler.ReturnCondition(cond);
            }
            Conditions.Clear();
        }

        public string GetKeyword()
        {
            return Keyword;
        }

        public bool Evaluate(KeywordContext<T> context)
        {
            foreach (ICondition<T> cond in Conditions)
            {
                if (cond.Evaluate(context))
                    return false;
            }

            return true;
        }

        public void Save(ScoutSerializer<T> serializer)
        {
            serializer.SaveConditionList(Conditions, false);
        }

        public void Load(ScoutSerializer<T> serializer)
        {
            serializer.LoadConditionList(Conditions, false);
        }
    }
}
