using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace UT_PathEarlViz
{
    public class TestTileInfo : ITileInfo
    {
        public bool IsForest = false;

        public void Clear()
        {
            IsForest = false;
        }

        public static Func<TestTileInfo> Spawner = () =>
        {
            return new TestTileInfo();
        };
    }
}
