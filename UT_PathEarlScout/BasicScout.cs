using PathEarlCore;
using PathEarlScout;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;
using System.Text;

namespace UT_PathEarlScout
{
    public static class BasicScout
    {
        public static ScoutRecycler<BasicTileInfo> Recycler = new ScoutRecycler<BasicTileInfo>();
        public static MapScratch<BasicTileInfo> MapScratch = new MapScratch<BasicTileInfo>();
        public static InfoAccess<BasicTileInfo> InfoAccess = BasicInfoAccess.GetInfoAccess();

        public static Scout<BasicTileInfo> MakeScout(Map<BasicTileInfo> map, string name)
        {
            Scout<BasicTileInfo> scout = new Scout<BasicTileInfo>(map, MapScratch, Recycler, InfoAccess);
            scout.Name = name;
            return scout;
        }

        public static Map<BasicTileInfo> MakeHexMap(int length, int height, out BasicTileInfo[,] infos)
        {
            Map<BasicTileInfo> map = new Map<BasicTileInfo>(() => { return new BasicTileInfo(); });
            map.GenerateGrid(length, height, out infos, null, hex: true);
            return map;
        }
    }
}
