using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class ScoutFuncManager<T> where T : ITileInfo
    {
        public Dictionary<string, Func<T, T, bool>> AdjacentFuncs = new Dictionary<string, Func<T, T, bool>>();
    }
}
