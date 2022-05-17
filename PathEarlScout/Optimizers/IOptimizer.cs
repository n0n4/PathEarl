using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Optimizers
{
    public interface IOptimizer<T> where T : ITileInfo
    {
        string GetKeyword();
        void SetSeed(int seed);
        void RunLayer(Scout<T> scout, Layer<T> layer, List<Rule<T>> globals);
    }
}
