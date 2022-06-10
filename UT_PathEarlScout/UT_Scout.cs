using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathEarlCore;
using PathEarlPixelLabInterface;
using PathEarlScout;
using PathEarlViz;
using System;
using UT_PathEarlScout.TestScouts;

namespace UT_PathEarlScout
{
    [TestClass]
    public class UT_Scout
    {
        private byte[] BackColor = new byte[] { 0, 0, 0, 255 };
        private byte[] LineColor = new byte[] { 100, 100, 100, 255 };
        private byte[][] DotColors = new byte[][]{
            new byte[] { 0, 0, 0, 255 }, // void
            new byte[] { 102, 181, 71, 255 }, // grass 1
            new byte[] { 135, 103, 45, 255 }, // dirt 2
            new byte[] { 235, 204, 131, 255 }, // desert 3
            new byte[] { 245, 242, 196, 255 }, // beach 4
            new byte[] { 138, 191, 179, 255 }, // shallows 5
            new byte[] { 83, 143, 184, 255 }, // water 6
            new byte[] { 34, 60, 143, 255 }, // ocean 7
            new byte[] { 26, 112, 85, 255 }, // swamp 8
            new byte[] { 184, 168, 167, 255 }, // ruins
            new byte[] { 105, 97, 97, 255 }, // walls
            new byte[] { 94, 52, 0, 255 }, // gate
            new byte[] { 83, 143, 184, 255 }, // river
        };
        private byte[] Coloration(Map<BasicTileInfo> map, int id)
        {
            BasicTileInfo info = map.Info[id];
            byte[] col = DotColors[info.TileType];

            return col;
        }

        public void RunTest(string name, int w, int h, Action<Scout<BasicTileInfo>> setup)
        {
            Map<BasicTileInfo> map = BasicScout.MakeHexMap(w, h, out BasicTileInfo[,] infos);
            Scout<BasicTileInfo> scout = BasicScout.MakeScout(map, name);
            setup(scout);
            scout.Run();

            Bitmap b = MapViz<BasicTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);

            // draw extras
            byte[] forestCol = new byte[] { 50, 150, 50, 255 };
            byte[] hillCol = new byte[] { 70, 70, 70, 255 };
            byte[] mountainCol = new byte[] { 0, 0, 0, 255 };
            foreach (var kvp in map.Info)
            {
                int relx = MapViz<BasicTileInfo>.FindRelativePosition(map.NodeX[kvp.Key], b.SmallestX, b.Resolution);
                int rely = MapViz<BasicTileInfo>.FindRelativePosition(map.NodeY[kvp.Key], b.SmallestY, b.Resolution);
                relx -= 5;
                rely -= 5;

                if (kvp.Value.TileHeight == 1) // hills
                {
                    b.DrawLine(relx + 2, rely + 7, relx + 5, rely + 2, hillCol);
                    b.DrawLine(relx + 8, rely + 7, relx + 5, rely + 2, hillCol);
                }
                else if (kvp.Value.TileHeight == 2) // mountains
                {
                    b.DrawLine(relx + 1, rely + 10, relx + 5, rely + 1, mountainCol);
                    b.DrawLine(relx + 9, rely + 10, relx + 5, rely + 1, mountainCol);
                    b.DrawLine(relx + 2, rely + 10, relx + 5, rely + 2, mountainCol);
                    b.DrawLine(relx + 7, rely + 10, relx + 5, rely + 2, mountainCol);
                }

                if (kvp.Value.TileFeature == 1) // forest
                {
                    b.SetPixel(relx, rely, forestCol, 1);
                    b.SetPixel(relx + 6, rely + 3, forestCol, 1);
                    b.SetPixel(relx + 3, rely + 6, forestCol, 1);
                }
            }

            ImageSharpExport.Save("./" + name + "-run.png", b);
        }

        public void SaveLoadRunTest(string name, int w, int h, Action<Scout<BasicTileInfo>> setup)
        {
            Scout<BasicTileInfo> loadScout = UT_ScoutSerializer.SaveLoad(name, setup, out Scout<BasicTileInfo> scout);

            RunTest(name + "-save-load", w, h, setup);
        }

        [TestMethod]
        public void StandardMap_RunTest()
        {
            RunTest("standard-map", 100, 100, (scout) =>
            {
                StandardMapScout.AddRules(scout);
            });
        }

        [TestMethod]
        public void GlobalVar_RunTest()
        {
            RunTest("global-var", 100, 100, (scout) =>
            {
                GlobalVarScout.AddRules(scout);
            });
        }
        
        [TestMethod]
        public void DynamicBracket_RunTest()
        {
            RunTest("dynamic-bracket", 100, 100, (scout) =>
            {
                DynamicBracketScout.AddRules(scout);
            });
        }

        [TestMethod]
        public void StandardMap_SaveLoadRunTest()
        {
            SaveLoadRunTest("standard-map", 100, 100, (scout) =>
            {
                StandardMapScout.AddRules(scout);
            });
        }

        [TestMethod]
        public void GlobalVar_SaveLoadRunTest()
        {
            SaveLoadRunTest("global-var", 100, 100, (scout) =>
            {
                GlobalVarScout.AddRules(scout);
            });
        }

        [TestMethod]
        public void DynamicBracket_SaveLoadRunTest()
        {
            SaveLoadRunTest("dynamic-bracket", 100, 100, (scout) =>
            {
                DynamicBracketScout.AddRules(scout);
            });
        }
    }
}
