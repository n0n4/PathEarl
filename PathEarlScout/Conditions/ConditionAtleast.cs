using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionAtleast<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "atleast";
        public List<ICondition<T>> Conditions;
        public int Count = 1;

        public ConditionAtleast(ScoutRecycler<T> recycler)
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
            int count = 0;
            foreach (ICondition<T> cond in Conditions)
            {
                if (cond.Evaluate(context))
                    count++;

                if (count >= Count)
                    return true;
            }

            return false;
        }

        public void Save(ScoutSerializer<T> serializer)
        {
            serializer.Write("COUNT ");
            serializer.WriteLine(Count.ToString());

            serializer.SaveConditionList(Conditions, false);
        }

        public void Load(ScoutSerializer<T> serializer)
        {
            serializer.TryReadLine();
            if (serializer.LastLine.Length < 7 || !serializer.LastLine.ToLower().StartsWith("count "))
                throw new Exception("Expected Count at line " + serializer.LineCount);
            string countText = serializer.LastLine.Substring(6);
            if (!int.TryParse(countText, out Count))
                throw new Exception("Expected numerical Count at line " + serializer.LineCount);

            serializer.LoadConditionList(Conditions, false);
        }
    }
}
