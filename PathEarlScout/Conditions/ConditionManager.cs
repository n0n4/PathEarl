using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public static class ConditionManager<T> where T : ITileInfo
    {
        public static Dictionary<string, Func<ScoutRecycler<T>, ICondition<T>>> CustomConditions = new Dictionary<string, Func<ScoutRecycler<T>, ICondition<T>>>();
        public static ICondition<T> GetNew(string keyword, ScoutRecycler<T> recycler)
        {
            switch (keyword)
            {
                case ConditionAnd<T>.Keyword:
                    return new ConditionAnd<T>(recycler);
                case ConditionOr<T>.Keyword:
                    return new ConditionOr<T>(recycler);
                case ConditionAtleast<T>.Keyword:
                    return new ConditionAtleast<T>(recycler);
                case ConditionNot<T>.Keyword:
                    return new ConditionNot<T>(recycler);
                case ConditionAdjacent<T>.Keyword:
                    return new ConditionAdjacent<T>(recycler);
                case ConditionIf<T>.Keyword:
                    return new ConditionIf<T>(recycler);
                case ConditionAlways<T>.Keyword:
                    return new ConditionAlways<T>(recycler);
                case ConditionDirection<T>.Keyword:
                    return new ConditionDirection<T>(recycler);
                default:
                    break;
            }

            if (CustomConditions.TryGetValue(keyword, out var condFunc))
            {
                return condFunc(recycler);
            }

            throw new Exception("Condition '" + keyword + "' not found");
        }
    }
}
