using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore
{
    public struct Tile<T> where T : ITileInfo
    {
        public readonly int Id;
        public readonly float X;
        public readonly float Y;
        public readonly EMapLayer Blocks;
        public T Info;

        public Tile(int id, float x, float y, EMapLayer blocks, T info)
        {
            Id = id;
            X = x;
            Y = y;
            Blocks = blocks;
            Info = info;
        }

        public Tile(Map<T> map, int id)
        {
            Id = id;
            X = map.NodeX[id];
            Y = map.NodeY[id];
            map.Blocks.TryGetValue(id, out Blocks);
            Info = map.Info[id];
        }   
    }
}
