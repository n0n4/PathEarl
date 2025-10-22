using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathEarlCore;
using PathEarlViz;
using System;
using System.Collections.Generic;

namespace UT_PathEarlViz
{
    [TestClass]
    public class UT_Bitmap
    {
        private byte[] BackColor = new byte[] { 0, 0, 0, 255 };
        private byte[] LineColor = new byte[] { 100, 100, 100, 255 };
        private byte[] DotColor = new byte[] { 0, 155, 0, 255 };
        private byte[] DotColor_GroundBlocked = new byte[] { 125, 125, 125, 255 };
        private byte[] DotColor_SeaBlocked = new byte[] { 0, 0, 155, 255 };
        private byte[] Coloration(Map<EmptyTileInfo> map, int id)
        {
            byte[] col = DotColor;
            if (map.IsBlocked(id, EMapLayer.Ground))
                col = DotColor_GroundBlocked;
            else if (map.IsBlocked(id, EMapLayer.Sea))
                col = DotColor_SeaBlocked;
            return col;
        }
        private byte[] Coloration(Map<TestTileInfo> map, int id)
        {
            byte[] col = DotColor;
            if (map.IsBlocked(id, EMapLayer.Ground))
                col = DotColor_GroundBlocked;
            else if (map.IsBlocked(id, EMapLayer.Sea))
                col = DotColor_SeaBlocked;
            return col;
        }

        [TestMethod]
        public void TestMap()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            gaps[5, 5] = true;
            gaps[5, 6] = true;
            gaps[5, 7] = true;
            gaps[5, 8] = true;
            gaps[5, 9] = true;
            gaps[5, 10] = true;
            gaps[6, 10] = true;
            gaps[6, 11] = true;
            gaps[6, 12] = true;
            gaps[6, 13] = true;
            gaps[7, 13] = true;
            gaps[8, 13] = true;
            gaps[9, 13] = true;
            gaps[10, 13] = true;
            gaps[11, 13] = true;
            gaps[12, 13] = true;
            gaps[12, 14] = true;
            gaps[12, 15] = true;
            gaps[12, 16] = true;
            gaps[12, 17] = true;
            gaps[12, 18] = true;

            gaps[21, 21] = true;
            gaps[21, 22] = true;
            gaps[21, 23] = true;
            gaps[21, 24] = true;
            gaps[21, 25] = true;
            gaps[21, 26] = true;
            gaps[21, 27] = true;
            gaps[21, 28] = true;
            gaps[21, 29] = true;
            gaps[22, 21] = true;
            gaps[23, 21] = true;
            gaps[24, 21] = true;
            gaps[25, 21] = true;
            gaps[26, 21] = true;
            gaps[27, 21] = true;
            gaps[27, 21] = true;
            gaps[27, 21] = true;
            gaps[23, 20] = true;
            gaps[23, 19] = true;
            gaps[23, 18] = true;
            gaps[23, 17] = true;
            gaps[23, 16] = true;
            gaps[23, 15] = true;

            int[,] ids = map.GenerateGrid(30, 30, out EmptyTileInfo[,] infos, gaps);

            int start = ids[2, 9];
            int target = ids[24, 24];

            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);
            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            MapViz<EmptyTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.None, scratch), new byte[] { 255, 0, 0, 255 }, 0);

            b.ToDrawingBitmap().Save("./testmap.png");
        }

        [TestMethod]
        public void TestHexMap()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            gaps[5, 5] = true;
            gaps[5, 6] = true;
            gaps[5, 7] = true;
            gaps[5, 8] = true;
            gaps[5, 9] = true;
            gaps[5, 10] = true;
            gaps[6, 10] = true;
            gaps[6, 11] = true;
            gaps[6, 12] = true;
            gaps[6, 13] = true;
            gaps[7, 13] = true;
            gaps[8, 13] = true;
            gaps[9, 13] = true;
            gaps[10, 13] = true;
            gaps[11, 13] = true;
            gaps[12, 13] = true;
            gaps[12, 14] = true;
            gaps[12, 15] = true;
            gaps[12, 16] = true;
            gaps[12, 17] = true;
            gaps[12, 18] = true;

            gaps[21, 21] = true;
            gaps[21, 22] = true;
            gaps[21, 23] = true;
            gaps[21, 24] = true;
            gaps[21, 25] = true;
            gaps[21, 26] = true;
            gaps[21, 27] = true;
            gaps[21, 28] = true;
            gaps[21, 29] = true;
            gaps[22, 21] = true;
            gaps[23, 21] = true;
            gaps[24, 21] = true;
            gaps[25, 21] = true;
            gaps[26, 21] = true;
            gaps[27, 21] = true;
            gaps[27, 21] = true;
            gaps[27, 21] = true;
            gaps[23, 20] = true;
            gaps[23, 19] = true;
            gaps[23, 18] = true;
            gaps[23, 17] = true;
            gaps[23, 16] = true;
            gaps[23, 15] = true;

            int[,] ids = map.GenerateGrid(30, 30, out EmptyTileInfo[,] infos, gaps, 2, 2, hex: true);

            int start = ids[2, 9];
            int target = ids[24, 24];

            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);
            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            MapViz<EmptyTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.None, scratch), new byte[] { 255, 0, 0, 255 }, 0);

            b.ToDrawingBitmap().Save("./testhexmap.png");
        }

        [TestMethod]
        public void TestHexMap_Blocks()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            int[,] ids = map.GenerateGrid(30, 30, out EmptyTileInfo[,] infos, gaps, 2, 2, hex: true);

            map.Block(ids[5, 5], EMapLayer.Ground);
            map.Block(ids[5, 6], EMapLayer.Ground);
            map.Block(ids[5, 7], EMapLayer.Ground);
            map.Block(ids[5, 8], EMapLayer.Ground);
            map.Block(ids[5, 9], EMapLayer.Ground);
            map.Block(ids[5, 10], EMapLayer.Ground);
            map.Block(ids[6, 10], EMapLayer.Ground);
            map.Block(ids[6, 11], EMapLayer.Ground);
            map.Block(ids[6, 12], EMapLayer.Ground);
            map.Block(ids[6, 13], EMapLayer.Ground);
            map.Block(ids[7, 13], EMapLayer.Ground);
            map.Block(ids[8, 13], EMapLayer.Ground);
            map.Block(ids[9, 13], EMapLayer.Ground);
            map.Block(ids[10, 13], EMapLayer.Ground);
            map.Block(ids[11, 13], EMapLayer.Ground);
            map.Block(ids[12, 13], EMapLayer.Ground);
            map.Block(ids[12, 14], EMapLayer.Ground);
            map.Block(ids[12, 15], EMapLayer.Ground);
            map.Block(ids[12, 16], EMapLayer.Ground);
            map.Block(ids[12, 17], EMapLayer.Ground);
            map.Block(ids[12, 18], EMapLayer.Ground);

            map.Block(ids[21, 21], EMapLayer.Sea);
            map.Block(ids[21, 22], EMapLayer.Sea);
            map.Block(ids[21, 23], EMapLayer.Sea);
            map.Block(ids[21, 24], EMapLayer.Sea);
            map.Block(ids[21, 25], EMapLayer.Sea);
            map.Block(ids[21, 26], EMapLayer.Sea);
            map.Block(ids[21, 27], EMapLayer.Sea);
            map.Block(ids[21, 28], EMapLayer.Sea);
            map.Block(ids[21, 29], EMapLayer.Sea);
            map.Block(ids[22, 21], EMapLayer.Sea);
            map.Block(ids[23, 21], EMapLayer.Sea);
            map.Block(ids[24, 21], EMapLayer.Sea);
            map.Block(ids[25, 21], EMapLayer.Sea);
            map.Block(ids[26, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[23, 20], EMapLayer.Sea);
            map.Block(ids[23, 19], EMapLayer.Sea);
            map.Block(ids[23, 18], EMapLayer.Sea);
            map.Block(ids[23, 17], EMapLayer.Sea);
            map.Block(ids[23, 16], EMapLayer.Sea);
            map.Block(ids[23, 15], EMapLayer.Sea);

            int start = ids[2, 9];
            int target = ids[24, 24];

            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);
            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            MapViz<EmptyTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.None, scratch), new byte[] { 255, 0, 0, 255 }, 0);
            MapViz<EmptyTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.Ground, scratch), new byte[] { 255, 155, 0, 255 }, 4);
            MapViz<EmptyTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.Ground | EMapLayer.Sea, scratch), new byte[] { 255, 0, 155, 255 }, -4);

            b.ToDrawingBitmap().Save("./testhexmapblocks.png");
        }

        [TestMethod]
        public void TestHexMap_Flowfield()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            int[,] ids = map.GenerateGrid(30, 30, out EmptyTileInfo[,] infos, gaps, 2, 2, hex: true);

            map.Block(ids[5, 5], EMapLayer.Ground);
            map.Block(ids[5, 6], EMapLayer.Ground);
            map.Block(ids[5, 7], EMapLayer.Ground);
            map.Block(ids[5, 8], EMapLayer.Ground);
            map.Block(ids[5, 9], EMapLayer.Ground);
            map.Block(ids[5, 10], EMapLayer.Ground);
            map.Block(ids[6, 10], EMapLayer.Ground);
            map.Block(ids[6, 11], EMapLayer.Ground);
            map.Block(ids[6, 12], EMapLayer.Ground);
            map.Block(ids[6, 13], EMapLayer.Ground);
            map.Block(ids[7, 13], EMapLayer.Ground);
            map.Block(ids[8, 13], EMapLayer.Ground);
            map.Block(ids[9, 13], EMapLayer.Ground);
            map.Block(ids[10, 13], EMapLayer.Ground);
            map.Block(ids[11, 13], EMapLayer.Ground);
            map.Block(ids[12, 13], EMapLayer.Ground);
            map.Block(ids[12, 14], EMapLayer.Ground);
            map.Block(ids[12, 15], EMapLayer.Ground);
            map.Block(ids[12, 16], EMapLayer.Ground);
            map.Block(ids[12, 17], EMapLayer.Ground);
            map.Block(ids[12, 18], EMapLayer.Ground);

            map.Block(ids[21, 21], EMapLayer.Sea);
            map.Block(ids[21, 22], EMapLayer.Sea);
            map.Block(ids[21, 23], EMapLayer.Sea);
            map.Block(ids[21, 24], EMapLayer.Sea);
            map.Block(ids[21, 25], EMapLayer.Sea);
            map.Block(ids[21, 26], EMapLayer.Sea);
            map.Block(ids[21, 27], EMapLayer.Sea);
            map.Block(ids[21, 28], EMapLayer.Sea);
            map.Block(ids[21, 29], EMapLayer.Sea);
            map.Block(ids[22, 21], EMapLayer.Sea);
            map.Block(ids[23, 21], EMapLayer.Sea);
            map.Block(ids[24, 21], EMapLayer.Sea);
            map.Block(ids[25, 21], EMapLayer.Sea);
            map.Block(ids[26, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[23, 20], EMapLayer.Sea);
            map.Block(ids[23, 19], EMapLayer.Sea);
            map.Block(ids[23, 18], EMapLayer.Sea);
            map.Block(ids[23, 17], EMapLayer.Sea);
            map.Block(ids[23, 16], EMapLayer.Sea);
            map.Block(ids[23, 15], EMapLayer.Sea);

            int start = ids[2, 9];

            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);
            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            MapViz<EmptyTileInfo>.DrawFlowfield(b, map, map.DjikstraFlowfield(start, EMapLayer.Ground | EMapLayer.Sea, scratch), new byte[] { 255, 0, 0, 255 }, 0);

            b.ToDrawingBitmap().Save("./testhexmapflowfield.png");
        }

        [TestMethod]
        public void TestHexMap_Search()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            int[,] ids = map.GenerateGrid(30, 30, out EmptyTileInfo[,] infos, gaps, 2, 2, hex: true);

            map.Block(ids[5, 5], EMapLayer.Ground);
            map.Block(ids[5, 6], EMapLayer.Ground);
            map.Block(ids[5, 7], EMapLayer.Ground);
            map.Block(ids[5, 8], EMapLayer.Ground);
            map.Block(ids[5, 9], EMapLayer.Ground);
            map.Block(ids[5, 10], EMapLayer.Ground);
            map.Block(ids[6, 10], EMapLayer.Ground);
            map.Block(ids[6, 11], EMapLayer.Ground);
            map.Block(ids[6, 12], EMapLayer.Ground);
            map.Block(ids[6, 13], EMapLayer.Ground);
            map.Block(ids[7, 13], EMapLayer.Ground);
            map.Block(ids[8, 13], EMapLayer.Ground);
            map.Block(ids[9, 13], EMapLayer.Ground);
            map.Block(ids[10, 13], EMapLayer.Ground);
            map.Block(ids[11, 13], EMapLayer.Ground);
            map.Block(ids[12, 13], EMapLayer.Ground);
            map.Block(ids[12, 14], EMapLayer.Ground);
            map.Block(ids[12, 15], EMapLayer.Ground);
            map.Block(ids[12, 16], EMapLayer.Ground);
            map.Block(ids[12, 17], EMapLayer.Ground);
            map.Block(ids[12, 18], EMapLayer.Ground);

            map.Block(ids[21, 21], EMapLayer.Sea);
            map.Block(ids[21, 22], EMapLayer.Sea);
            map.Block(ids[21, 23], EMapLayer.Sea);
            map.Block(ids[21, 24], EMapLayer.Sea);
            map.Block(ids[21, 25], EMapLayer.Sea);
            map.Block(ids[21, 26], EMapLayer.Sea);
            map.Block(ids[21, 27], EMapLayer.Sea);
            map.Block(ids[21, 28], EMapLayer.Sea);
            map.Block(ids[21, 29], EMapLayer.Sea);
            map.Block(ids[22, 21], EMapLayer.Sea);
            map.Block(ids[23, 21], EMapLayer.Sea);
            map.Block(ids[24, 21], EMapLayer.Sea);
            map.Block(ids[25, 21], EMapLayer.Sea);
            map.Block(ids[26, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[23, 20], EMapLayer.Sea);
            map.Block(ids[23, 19], EMapLayer.Sea);
            map.Block(ids[23, 18], EMapLayer.Sea);
            map.Block(ids[23, 17], EMapLayer.Sea);
            map.Block(ids[23, 16], EMapLayer.Sea);
            map.Block(ids[23, 15], EMapLayer.Sea);

            int start = ids[2, 9];
            int above = ids[2, 8];

            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);
            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            float min = 2f;
            float max = 5f;
            MapViz<EmptyTileInfo>.DrawSearch(b, map, max, map.DjikstraSearch(start, min, max, EMapLayer.Ground | EMapLayer.Sea, scratch), new byte[] { 255, 0, 0, 255 }, 0);

            b.ToDrawingBitmap().Save("./testhexmapsearch.png");
        }

        [TestMethod]
        public void TestHexMap_TestTileInfoForest()
        {
            Map<TestTileInfo> map = new Map<TestTileInfo>(TestTileInfo.Spawner);

            bool[,] gaps = new bool[30, 30];
            int[,] ids = map.GenerateGrid(30, 30, out TestTileInfo[,] infos, gaps, 2, 2, hex: true);

            map.Block(ids[5, 5], EMapLayer.Ground);
            map.Block(ids[5, 6], EMapLayer.Ground);
            map.Block(ids[5, 7], EMapLayer.Ground);
            map.Block(ids[5, 8], EMapLayer.Ground);
            map.Block(ids[5, 9], EMapLayer.Ground);
            map.Block(ids[5, 10], EMapLayer.Ground);
            map.Block(ids[6, 10], EMapLayer.Ground);
            map.Block(ids[6, 11], EMapLayer.Ground);
            map.Block(ids[6, 12], EMapLayer.Ground);
            map.Block(ids[6, 13], EMapLayer.Ground);
            map.Block(ids[7, 13], EMapLayer.Ground);
            map.Block(ids[8, 13], EMapLayer.Ground);
            map.Block(ids[9, 13], EMapLayer.Ground);
            map.Block(ids[10, 13], EMapLayer.Ground);
            map.Block(ids[11, 13], EMapLayer.Ground);
            map.Block(ids[12, 13], EMapLayer.Ground);
            map.Block(ids[12, 14], EMapLayer.Ground);
            map.Block(ids[12, 15], EMapLayer.Ground);
            map.Block(ids[12, 16], EMapLayer.Ground);
            map.Block(ids[12, 17], EMapLayer.Ground);
            map.Block(ids[12, 18], EMapLayer.Ground);

            map.Block(ids[21, 21], EMapLayer.Sea);
            map.Block(ids[21, 22], EMapLayer.Sea);
            map.Block(ids[21, 23], EMapLayer.Sea);
            map.Block(ids[21, 24], EMapLayer.Sea);
            map.Block(ids[21, 25], EMapLayer.Sea);
            map.Block(ids[21, 26], EMapLayer.Sea);
            map.Block(ids[21, 27], EMapLayer.Sea);
            map.Block(ids[21, 28], EMapLayer.Sea);
            map.Block(ids[21, 29], EMapLayer.Sea);
            map.Block(ids[22, 21], EMapLayer.Sea);
            map.Block(ids[23, 21], EMapLayer.Sea);
            map.Block(ids[24, 21], EMapLayer.Sea);
            map.Block(ids[25, 21], EMapLayer.Sea);
            map.Block(ids[26, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[27, 21], EMapLayer.Sea);
            map.Block(ids[23, 20], EMapLayer.Sea);
            map.Block(ids[23, 19], EMapLayer.Sea);
            map.Block(ids[23, 18], EMapLayer.Sea);
            map.Block(ids[23, 17], EMapLayer.Sea);
            map.Block(ids[23, 16], EMapLayer.Sea);
            map.Block(ids[23, 15], EMapLayer.Sea);

            List<int> forestIds = new List<int>()
            {
                ids[11, 18], ids[12, 18], ids[13, 18], ids[14, 18],
                ids[11, 19], ids[12, 19], ids[13, 19], ids[14, 19],
                ids[11, 20], ids[12, 20], ids[13, 20], ids[14, 20],
                ids[12, 21],
                ids[12, 22],
                ids[12, 23],
                ids[12, 24],
                ids[12, 25],
            };
            foreach (int id in forestIds)
                map.Info[id].IsForest = true;

            Func<int, int, float, TestTileInfo, TestTileInfo, float> cost = (from, to, dist, fromInfo, toInfo) =>
            {
                // moving into forests costs an extra tile of movement
                if (toInfo.IsForest)
                    return dist + 1;

                return dist;
            };

            int start = ids[2, 9];
            int target = ids[24, 24];

            Bitmap b = MapViz<TestTileInfo>.DrawMap(map, 10, BackColor, LineColor, Coloration);

            // draw the forests
            byte[] col = new byte[] { 100, 200, 100, 255 };
            foreach (int id in forestIds)
            {
                int relx = MapViz<TestTileInfo>.FindRelativePosition(map.NodeX[id], b.SmallestX, b.Resolution);
                int rely = MapViz<TestTileInfo>.FindRelativePosition(map.NodeY[id], b.SmallestY, b.Resolution);

                b.SetPixel(relx, rely, col, 3);
            }

            MapScratch<TestTileInfo> scratch = new MapScratch<TestTileInfo>();
            MapViz<TestTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.None, scratch, cost), new byte[] { 255, 0, 0, 255 }, 0);
            MapViz<TestTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.Ground, scratch, cost), new byte[] { 255, 155, 0, 255 }, 4);
            MapViz<TestTileInfo>.DrawPath(b, map, map.Djikstra(start, target, EMapLayer.Ground | EMapLayer.Sea, scratch, cost), new byte[] { 255, 0, 155, 255 }, -4);

            b.ToDrawingBitmap().Save("./testhexmaptileinfoforest.png");
        }
    }
}
