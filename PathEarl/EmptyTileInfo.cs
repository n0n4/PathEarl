using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore
{
    public class EmptyTileInfo : ITileInfo
    {
        public void Clear()
        {
        }

        public static Func<EmptyTileInfo> Spawner = () =>
        {
            return new EmptyTileInfo();
        };
    }
}
