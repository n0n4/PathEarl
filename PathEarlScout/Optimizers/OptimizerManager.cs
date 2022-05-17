using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Optimizers
{
    public static class OptimizerManager<T> where T : ITileInfo
    {
        public static Dictionary<string, Func<ScoutRecycler<T>, IOptimizer<T>>> CustomOptimizers = new Dictionary<string, Func<ScoutRecycler<T>, IOptimizer<T>>>();
        public static IOptimizer<T> GetNew(string keyword, ScoutRecycler<T> recycler)
        {
            switch (keyword)
            {
                case SimpleOptimizer<T>.Keyword:
                    return new SimpleOptimizer<T>();
                default: 
                    break;
            }

            if (CustomOptimizers.TryGetValue(keyword, out var optFunc))
            {
                return optFunc(recycler);
            }

            throw new Exception("Optimizer '" + keyword + "' not found");
        }
    }
}
