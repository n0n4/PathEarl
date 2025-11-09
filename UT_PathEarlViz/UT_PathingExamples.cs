using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathEarlCore;
using PathEarlViz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT_PathEarlViz
{
    [TestClass]
    public class UT_PathingExamples
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

        [TestMethod]
        public void TestMap_StrangeShapedObjectTest()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            // . . o X . . . . .
            // o . o X . . . . .
            // . X . . . X . . .
            // . v . X X o X . .
            // . v . o . o . . .
            // . > > > ^ . . . .
            // . . . . . . . . .


            int[,] ids = map.GenerateGrid(9, 7, out EmptyTileInfo[,] infos, null);

            int start = ids[1, 1];
            int target = ids[4, 4];

            map.Block(ids[1, 2], EMapLayer.Ground);
            map.Block(ids[3, 0], EMapLayer.Ground);
            map.Block(ids[3, 1], EMapLayer.Ground);
            map.Block(ids[3, 3], EMapLayer.Ground);
            map.Block(ids[4, 3], EMapLayer.Ground);
            map.Block(ids[5, 2], EMapLayer.Ground);
            map.Block(ids[6, 3], EMapLayer.Ground);

            map.SetupQuad();

            List<(float, float)> shape = new List<(float, float)>()
            {
                (-1f, 0f), (1f, 0f), (1f, -1f)
            };

            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 20, BackColor, LineColor, Coloration);
            List<int> path = map.Djikstra(start, target, EMapLayer.Ground, scratch, null, shape);
            MapViz<EmptyTileInfo>.DrawPath(b, map, path, new byte[] { 255, 0, 0, 255 }, 0);

            byte[] shapeColor = new byte[] { 255, 0, 155, 255 };

            foreach (var id in new int[] { start, target })
            {
                foreach (var pos in shape)
                {
                    int relx = MapViz<TestTileInfo>.FindRelativePosition(map.NodeX[id] + pos.Item1, b.SmallestX, b.Resolution);
                    int rely = MapViz<TestTileInfo>.FindRelativePosition(map.NodeY[id] + pos.Item2, b.SmallestY, b.Resolution);

                    b.SetPixel(relx, rely, shapeColor, 3);
                }
            }

            b.ToDrawingBitmap().Save("./teststrangeobject1.png");

            // validate
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(ids[1, 2], path[1]);
            Assert.AreEqual(ids[1, 3], path[2]);
            Assert.AreEqual(ids[1, 4], path[3]);
            Assert.AreEqual(ids[1, 5], path[4]);
            Assert.AreEqual(ids[2, 5], path[5]);
            Assert.AreEqual(ids[3, 5], path[6]);
            Assert.AreEqual(ids[4, 5], path[7]);
            Assert.AreEqual(target, path[8]);
        }

        [TestMethod]
        public void TestMap_PathTowardsOccupiedSquareTest()
        {
            Map<EmptyTileInfo> map = new Map<EmptyTileInfo>(EmptyTileInfo.Spawner);

            // . > > v .
            // . ^ X O .
            // . O X 2 .
            // . . X . .
            // . . X . .

            int[,] ids = map.GenerateGrid(5, 5, out EmptyTileInfo[,] infos, null);

            int start = ids[1, 2];
            int target = ids[3, 2];

            map.Block(ids[2, 1], EMapLayer.Ground);
            map.Block(ids[2, 2], EMapLayer.Ground);
            map.Block(ids[2, 3], EMapLayer.Ground);
            map.Block(ids[2, 4], EMapLayer.Ground);

            map.Block(ids[3, 2], EMapLayer.Ground);

            map.SetupQuad();

            MapScratch<EmptyTileInfo> scratch = new MapScratch<EmptyTileInfo>();
            Bitmap b = MapViz<EmptyTileInfo>.DrawMap(map, 20, BackColor, LineColor, Coloration);


            var tempBlock = map.Blocks[target];
            map.Blocks[target] = EMapLayer.None;
            List<int> path = map.Djikstra(start, target, EMapLayer.Ground, scratch, null);
            map.Blocks[target] = tempBlock;

            MapViz<EmptyTileInfo>.DrawPath(b, map, path, new byte[] { 255, 0, 0, 255 }, 0);

            byte[] shapeColor = new byte[] { 255, 0, 155, 255 };

            b.ToDrawingBitmap().Save("./testpathtowardsoccupied.png");

            // validate
            Assert.AreEqual(7, path.Count);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(ids[1, 1], path[1]);
            Assert.AreEqual(ids[1, 0], path[2]);
            Assert.AreEqual(ids[2, 0], path[3]);
            Assert.AreEqual(ids[3, 0], path[4]);
            Assert.AreEqual(ids[3, 1], path[5]);
            Assert.AreEqual(target, path[6]);
        }
    }
}
