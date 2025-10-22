# PathEarlViz

Provides a means for making `Bitmap`s out of `Map`s, mostly for debugging purposes. First, define a coloration method. This tells the renderer what color to give each tile:

```cs
private byte[] DotColor = new byte[] { 0, 155, 0, 255 };
private byte[] DotColor_GroundBlocked = new byte[] { 125, 125, 125, 255 };
private byte[] DotColor_SeaBlocked = new byte[] { 0, 0, 155, 255 };

private byte[] Coloration(Map<TestTileInfo> map, int id)
{
    byte[] col = DotColor;
    if (map.IsBlocked(id, EMapLayer.Ground))
        col = DotColor_GroundBlocked;
    else if (map.IsBlocked(id, EMapLayer.Sea))
        col = DotColor_SeaBlocked;
    return col;
}
```

After that, we can draw the map:

```cs
byte[] backColor = new byte[] { 0, 0, 0, 255 };
byte[] lineColor = new byte[] { 100, 100, 100, 255 };

Bitmap bitmap = MapViz<TestTileInfo>.DrawMap(map, 10, backColor, lineColor, Coloration);
```

Next, we can add graphics for any features we want to highlight manually, by using `FindRelativePosition`:

```cs
// e.g. if we defined some tiles as having forests:
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

// we can draw dots where the forests are
byte[] col = new byte[] { 100, 200, 100, 255 };
foreach (int id in forestIds)
{
    int relx = MapViz<TestTileInfo>.FindRelativePosition(map.NodeX[id], b.SmallestX, b.Resolution);
    int rely = MapViz<TestTileInfo>.FindRelativePosition(map.NodeY[id], b.SmallestY, b.Resolution);

    bitmap.SetPixel(relx, rely, col, 3);
}
```

Finally, we can draw paths over the map like so:

```cs

MapScratch<TestTileInfo> scratch = new MapScratch<TestTileInfo>();

List<int> pathIdsWalksFliesAndSwims = map.Djikstra(start, target, EMapLayer.None, scratch, cost);
List<int> pathIdsWalksAndSwims = map.Djikstra(start, target, EMapLayer.Ground, scratch, cost);
List<int> pathIdsWalks = map.Djikstra(start, target, EMapLayer.Ground | EMapLayer.Sea, scratch, cost);

MapViz<TestTileInfo>.DrawPath(b, map, pathIdsWalksFliesAndSwims, new byte[] { 255, 0, 0, 255 }, 0);
MapViz<TestTileInfo>.DrawPath(b, map, pathIdsWalksAndSwims, new byte[] { 255, 155, 0, 255 }, 4);
MapViz<TestTileInfo>.DrawPath(b, map, pathIdsWalks, new byte[] { 255, 0, 155, 255 }, -4);
```

We can convert this `Bitmap` to a `System.Drawing.Bitmap` and save it: 

```cs
b.ToDrawingBitmap().Save("./testforest.png");
```

Note that `ToDrawingBitmap` may only work on the Windows platform.