# PathEarlCore

First, define a TileInfo (or use EmptyTileInfo if you have no tile types / features to worry about). The TileInfo defines whatever parameters a tile may have, for instance whether it `HasMountain = true` or `TileType = "swamp"`, etc. See `BasicTileInfo` in `UT_PathEarlScout` for a thorough example.

```cs
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
```

Next, create a Map:

```cs
Map<TestTileInfo> map = new Map<TestTileInfo>(TestTileInfo.Spawner);
```

The `GenerateGrid` method builds out the base structure of the map:

```cs
// gaps can be used to create voids where there are no tiles
bool[,] gaps = new bool[30, 30];
gaps[5, 5] = true;
gaps[5, 6] = true;

// note that xdist/ydist are how far apart the rows/columns are in pathfinding cost
int[,] ids = map.GenerateGrid(30, 30, out TestTileInfo[,] infos, gaps, xdist: 1, ydist: 1, hex: true);
```

Note that you can also generate any arbitrary map rather than a grid by using the `AddNode` and `AddConnection` methods on map; see the `GenerateGrid` source for an example.

Each node has an `id` which is used to refer to it; the `ids` array that `GenerateGrid` produces gives a handy reference for the ids of all tiles in the grid.

You may add blocking (collision) to the map via the `Block` method:

```
map.Block(ids[12, 18], EMapLayer.Ground);

map.Block(ids[21, 21], EMapLayer.Sea);
```

If a layer is blocked on a tile, any unit with that blocking type cannot pass through it. There are six blocking layers by default, though their names are arbitrary.

You can adjust any tile parameters by accessing their info like so:

```cs
map.Info[ids[10,10]].IsForest = true;
```

You may define a travel cost function that makes traveling through certain tiles cost more than others:

```cs

Func<int, int, float, TestTileInfo, TestTileInfo, float> cost = (from, to, dist, fromInfo, toInfo) =>
{
    // moving into forests costs an extra tile of movement
    if (toInfo.IsForest)
        return dist + 1;

    return dist;
};
```

Finally, you can find a path with the `Djikstra` method:

```cs
int start = ids[2, 9];
int target = ids[24, 24];

// suggested that you persist one MapScratch and reuse it instead of making a new one each time
MapScratch<TestTileInfo> scratch = new MapScratch<TestTileInfo>();

List<int> pathIdsWalksFliesAndSwims = map.Djikstra(start, target, EMapLayer.None, scratch, cost);
List<int> pathIdsWalksAndSwims = map.Djikstra(start, target, EMapLayer.Ground, scratch, cost);
List<int> pathIdsWalks = map.Djikstra(start, target, EMapLayer.Ground | EMapLayer.Sea, scratch, cost);
```

The resultant list is the sequence of tiles (by their ids) that the unit will pass through. After a unit has moved, you can `Unblock` their old tile and `Block` their new tile so that other units cannot pass through them, if desired.