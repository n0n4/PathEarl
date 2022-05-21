using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace UT_PathEarlScout
{
    public class BasicTileInfo : ITileInfo
    {
        // for readability
        public static Dictionary<int, string> TileTypeToName = new Dictionary<int, string>()
        {
            {0, "void" },
            {1, "grass" },
            {2, "dirt" },
            {3, "desert" },
            {4, "beach" },
            {5, "shallows" },
            {6, "water" },
            {7, "ocean" },
            {8, "swamp" },
            {9, "ruins" },
            {10, "walls" },
            {11, "gate" },
            {12, "river" },
        };
        public static Dictionary<string, int> TileNameToType = new Dictionary<string, int>()
        {
            {"void", 0 },
            {"grass", 1 },
            {"dirt", 2 },
            {"desert", 3 },
            {"beach", 4 },
            {"shallows", 5 },
            {"water", 6 },
            {"ocean", 7 },
            {"swamp", 8 },
            {"ruins", 9 },
            {"walls", 10 },
            {"gate", 11 },
            {"river", 12 },
        };
        public static Dictionary<string, string> TileTypeToArchetype = new Dictionary<string, string>()
        {
            {"void", "none" },
            {"grass", "ground" },
            {"dirt", "ground" },
            {"desert", "ground" },
            {"beach", "ground" },
            {"shallows", "water" },
            {"water", "water" },
            {"ocean", "water" },
            {"swamp", "ground" },
            {"ruins", "ground" },
            {"walls", "ground" },
            {"gate", "ground" },
            {"river", "water" },
        };

        public static Dictionary<int, string> HeightToName = new Dictionary<int, string>()
        {
            {0, "flat" },
            {1, "hills" },
            {2, "mountains" }
        };
        public static Dictionary<string, int> NameToHeight = new Dictionary<string, int>()
        {
            {"flat", 0 },
            {"hills", 1 },
            {"mountains", 2 }
        };

        public static Dictionary<int, string> FeatureToName = new Dictionary<int, string>()
        {
            {0, "empty" },
            {1, "forest" }
        };
        public static Dictionary<string, int> NameToFeature = new Dictionary<string, int>()
        {
            {"empty", 0 },
            {"forest", 1 }
        };

        // local fields
        public int TileType = 0;
        public int TileHeight = 0;
        public int TileFeature = 0;
        public void Clear()
        {
            TileType = 0;
            TileHeight = 0;
            TileFeature = 0;
        }

        // accessors
        public static Action<Tile<BasicTileInfo>, string> SetTileType = (tile, type) =>
        {
            tile.Info.TileType = TileNameToType[type];
        };
        public static Func<Tile<BasicTileInfo>, string> GetTileType = (tile) =>
        {
            return TileTypeToName[tile.Info.TileType];
        };

        public static Action<Tile<BasicTileInfo>, string> SetTileHeight = (tile, type) =>
        {
            tile.Info.TileHeight = NameToHeight[type];
        };
        public static Func<Tile<BasicTileInfo>, string> GetTileHeight = (tile) =>
        {
            return HeightToName[tile.Info.TileHeight];
        };

        public static Action<Tile<BasicTileInfo>, string> SetTileFeature = (tile, type) =>
        {
            tile.Info.TileFeature = NameToFeature[type];
        };
        public static Func<Tile<BasicTileInfo>, string> GetTileFeature = (tile) =>
        {
            return FeatureToName[tile.Info.TileFeature];
        };

        public static Func<Tile<BasicTileInfo>, string> GetTileArchetype = (tile) =>
        {
            return TileTypeToArchetype[TileTypeToName[tile.Info.TileType]];
        };
    }
}
