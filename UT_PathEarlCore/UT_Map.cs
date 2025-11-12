using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathEarlCore;
using System.Collections.Generic;

namespace UT_PathEarlCore
{
    [TestClass]
    public class UT_Map
    {
        [TestMethod]
        public void TestMap_UTest()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            // 1X2    1X2
            // .X. -> vX^
            // ...    >>^

            bool[,] gaps = new bool[3, 3];
            gaps[1, 0] = true;
            gaps[1, 1] = true;

            int[,] ids = map.GenerateGrid(3, 3, out EmptyTileInfo[,] infos, gaps);

            int start = ids[0, 0];
            int target = ids[2, 0];

            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            List<int> path = map.Djikstra(start, target, EMapLayer.None, scratch);

            // validate
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(ids[0, 1], path[1]);
            Assert.AreEqual(ids[0, 2], path[2]);
            Assert.AreEqual(ids[1, 2], path[3]);
            Assert.AreEqual(ids[2, 2], path[4]);
            Assert.AreEqual(ids[2, 1], path[5]);
            Assert.AreEqual(ids[2, 0], path[6]);
        }

        [TestMethod]
        public void TestMap_PathOutOfOccupiedSquare()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[3, 3];
            gaps[1, 0] = true;
            gaps[1, 1] = true;

            int[,] ids = map.GenerateGrid(3, 3, out EmptyTileInfo[,] infos, gaps);

            int start = ids[0, 0];
            map.Block(ids[0, 0], EMapLayer.Ground);
            int target = ids[2, 0];

            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            List<int> path = map.Djikstra(start, target, EMapLayer.Ground, scratch);

            // validate
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(ids[0, 1], path[1]);
            Assert.AreEqual(ids[0, 2], path[2]);
            Assert.AreEqual(ids[1, 2], path[3]);
            Assert.AreEqual(ids[2, 2], path[4]);
            Assert.AreEqual(ids[2, 1], path[5]);
            Assert.AreEqual(ids[2, 0], path[6]);
        }

        [TestMethod]
        public void TestMap_NullIfCantPath()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            bool[,] gaps = new bool[3, 3];
            gaps[1, 0] = true;
            gaps[1, 1] = true;
            gaps[1, 2] = true;

            int[,] ids = map.GenerateGrid(3, 3, out EmptyTileInfo[,] infos, gaps);

            int start = ids[0, 0];
            int target = ids[2, 0];

            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            List<int> path = map.Djikstra(start, target, EMapLayer.Ground, scratch);

            // validate
            Assert.AreEqual(0, path.Count);
        }
    }
}
