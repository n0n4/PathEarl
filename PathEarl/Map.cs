using PathEarlCore.Quads;
using System;
using System.Collections.Generic;

namespace PathEarlCore
{
    public class Map<T> where T : ITileInfo
    {
        public Dictionary<int, Dictionary<int, float>> Nodes = new Dictionary<int, Dictionary<int, float>>();
        private List<Dictionary<int, float>> UsedNodeDicts = new List<Dictionary<int, float>>();
        private List<Dictionary<int, float>> UnusedNodeDicts = new List<Dictionary<int, float>>();
        public Dictionary<int, float> NodeX = new Dictionary<int, float>();
        public Dictionary<int, float> NodeY = new Dictionary<int, float>();
        public Dictionary<int, EMapLayer> Blocks = new Dictionary<int, EMapLayer>();
        public Dictionary<int, T> Info = new Dictionary<int, T>();
        public List<T> UnusedInfo = new List<T>();
        public List<T> UsedInfo = new List<T>();
        public Func<T> InfoSpawner;
        public int CurrentId = 0;
        public struct QuadCollisionChecker : IQuadCollisionChecker<TileQuadEntry>
        {
            public bool Collides(Quadentry<TileQuadEntry> a, Quadentry<TileQuadEntry> b)
            {
                if ((a.Data.Blocking & b.Data.Blocking) == 0
                    || a.X + a.W < b.X
                    || a.Y + a.H < b.Y
                    || a.X > b.X + b.W
                    || a.Y > b.Y + b.H)
                    return false;

                return true;
            }
        }
        public Dictionary<int, Quadentry<TileQuadEntry>> QuadEntries = new Dictionary<int, Quadentry<TileQuadEntry>>();
        public Quadhead<TileQuadEntry> Quadhead = null;
        public Quadchecker<TileQuadEntry, QuadCollisionChecker> Quadchecker = null;
        private Dictionary<EMapLayer, Quadentry<TileQuadEntry>> ScratchQuadEntries = new Dictionary<EMapLayer, Quadentry<TileQuadEntry>>();

        public Map(Func<T> infoSpawner)
        {
            InfoSpawner = infoSpawner;
        }

        public void SetupQuad()
        {
            if (Quadhead == null)
            {
                int maxNodes = NodeX.Count;
                if (maxNodes < 10000)
                    maxNodes = 10000;
                Quadhead = new Quadhead<TileQuadEntry>(new TileQuadEntry(), 2f, maxNodes / 4, maxNodes);
                Quadchecker = new Quadchecker<TileQuadEntry, QuadCollisionChecker>(Quadhead);
            }

            float x = float.MaxValue;
            float y = float.MaxValue;
            float w = float.MinValue;
            float h = float.MinValue;

            foreach (var kvp in NodeX)
            {
                if (kvp.Value < x)
                    x = kvp.Value;
                if (kvp.Value > w)
                    w = kvp.Value;
            }

            foreach (var kvp in NodeY)
            {
                if (kvp.Value < y)
                    y = kvp.Value;
                if (kvp.Value > h)
                    h = kvp.Value;
            }

            float radius = (w - x) / 10;
            if (radius < 2)
                radius = 2;

            Quadhead.Clear();
            Quadhead.StartTree(x - radius, y - radius, w + radius, h + radius);

            foreach (var kvp in Nodes)
            {
                int id = kvp.Key;
                float nodex = NodeX[id];
                float nodey = NodeY[id];

                float minsize = int.MaxValue;
                foreach (var connection in kvp.Value)
                {
                    if (connection.Value < minsize)
                    {
                        minsize = connection.Value;
                    }
                }

                EMapLayer blocking = EMapLayer.None;
                Blocks.TryGetValue(id, out blocking);
                TileQuadEntry entry = new TileQuadEntry()
                {
                    Blocking = blocking,
                    Id = id
                };

                // this method may be a bit awkward for strangely shaped maps
                QuadEntries[id] = Quadhead.Insert(entry, nodex - (minsize / 2f), nodey - (minsize / 2f), minsize, minsize);
            }
        }

        public Tile<T> GetTile(int id)
        {
            return new Tile<T>(this, id);
        }

        public void Clear()
        {
            CurrentId = 0;

            foreach (Dictionary<int, float> dict in UsedNodeDicts)
                dict.Clear();
            UnusedNodeDicts.AddRange(UsedNodeDicts);
            UsedNodeDicts.Clear();
            Nodes.Clear();

            NodeX.Clear();
            NodeY.Clear();
            Blocks.Clear();

            foreach (T info in UsedInfo)
                info.Clear();
            UnusedInfo.AddRange(UsedInfo);
            UsedInfo.Clear();
            Info.Clear();

            Quadhead.Clear();
            QuadEntries.Clear();
        }

        public int AddNode(float x, float y, out T info)
        {
            int id = CurrentId;
            CurrentId++;

            // get an unused nodedict, or make a new one if we don't have any
            Dictionary<int, float> nodeDict = null;
            if (UnusedNodeDicts.Count > 0)
            {
                nodeDict = UnusedNodeDicts[0];
                UnusedNodeDicts.RemoveAt(0);
            } 
            else
            {
                nodeDict = new Dictionary<int, float>();
            }

            // get an unused info, or make a new one if we don't have any
            if (UnusedInfo.Count > 0)
            {
                info = UnusedInfo[0];
                UnusedInfo.RemoveAt(0);
            }
            else
            {
                info = InfoSpawner();
            }

            Nodes.Add(id, nodeDict);
            UsedNodeDicts.Add(nodeDict);
            Info.Add(id, info);
            UsedInfo.Add(info);
            NodeX[id] = x;
            NodeY[id] = y;

            return id;
        }

        public void AddConnection(int id, int target, float? distance = null, bool symmetrical = true)
        {
            float realdist = 0;
            if(distance != null)
            {
                realdist = (float)distance;
            }
            else
            {
                // if a distance isn't provided, we will figure it out from the x/y coordinates
                float x1 = NodeX[id];
                float y1 = NodeY[id];
                float x2 = NodeX[target];
                float y2 = NodeY[target];

                realdist = (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            }
            Nodes[id][target] = realdist;
            // if a connection is symmetrical, we add the same connection going both ways
            if (symmetrical)
            {
                AddConnection(target, id, distance, false);
            }
        }

        public void RemoveConnection(int id, int target, bool symmetrical = true)
        {
            if (Nodes[id].ContainsKey(target))
                Nodes[id].Remove(target);

            if (symmetrical)
                RemoveConnection(target, id, false);
        }

        public void Block(int id, EMapLayer layer)
        {
            if (Blocks.TryGetValue(id, out EMapLayer blocking))
                Blocks[id] = blocking | layer;
            else
                Blocks.Add(id, layer);

            if (QuadEntries.TryGetValue(id, out Quadentry<TileQuadEntry> entry))
                entry.Data.Blocking = Blocks[id];
        }

        public bool IsBlocked(int id, EMapLayer layer)
        {
            if (Blocks.TryGetValue(id, out EMapLayer blocking))
                return (blocking & layer) != 0;
            return false;
        }

        private Quadentry<TileQuadEntry> GetScratchQuadEntry(EMapLayer layer)
        {
            if (ScratchQuadEntries.TryGetValue(layer, out var existingEntry))
                return existingEntry;

            Quadentry<TileQuadEntry> entry = new Quadentry<TileQuadEntry>(new TileQuadEntry());
            entry.Data.Blocking = layer;
            ScratchQuadEntries[layer] = entry;
            return entry;
        }

        public bool IsBlocked(int id, EMapLayer layer, List<(float, float)> shape, float radius = 0.5f)
        {
            var testEntry = GetScratchQuadEntry(layer);
            float origX = NodeX[id];
            float origY = NodeY[id];

            foreach (var pos in shape)
            {
                float newX = pos.Item1 + origX;
                float newY = pos.Item2 + origY;
                Quadentry<TileQuadEntry> hit = Quadchecker.CheckFirst(testEntry,
                    newX - (radius / 2f), newY - (radius / 2f), radius, radius);

                if (hit != null)
                    return true;
            }
            return false;
        }

        public void Unblock(int id, EMapLayer layer)
        {
            if (IsBlocked(id, layer))
                Blocks[id] &= ~layer;

            if (QuadEntries.TryGetValue(id, out Quadentry<TileQuadEntry> entry))
                entry.Data.Blocking = Blocks[id];
        }

        public void Unblock(int id)
        {
            Blocks[id] = EMapLayer.None;
        }

        public float GetAngle(int fromId, int toId)
        {
            float dx = NodeX[toId] - NodeX[fromId];
            float dy = NodeY[toId] - NodeY[fromId];
            return (float)Math.Atan2(dy, dx);
        }

        public float GetAngleDeg(int fromId, int toId)
        {
            return (float)(180f / Math.PI) * GetAngle(fromId, toId);
        }

        public int[,] GenerateGrid(int width, int height, out T[,] infos, bool[,] gaps = null, float xdist = 1, float ydist = 1, bool diags = false, bool hex = false)
        {
            int[,] ids = new int[width, height];
            infos = new T[width, height];

            // first make all the nodes
            int i = 0;
            while (i < width)
            {
                int o = 0;
                while (o < height)
                {
                    if (gaps == null || !gaps[i, o])
                    {
                        float y = ((float)o) * ydist;
                        if (hex && i % 2 == 0)
                            y += (ydist / 2);
                        ids[i, o] = AddNode(((float)i) * xdist, y, out T info);
                        infos[i, o] = info;
                    }
                    else
                    {
                        ids[i, o] = int.MaxValue;
                    }
                    o++;
                }
                i++;
            }

            // now make all the connections
            i = 0;
            while (i < width)
            {
                int o = 0;
                while (o < height)
                {
                    if (gaps == null || !gaps[i, o])
                    {
                        int a = -1;
                        while (a < 2)
                        {
                            int b = -1;
                            while (b < 2)
                            {
                                if ((a != 0 || b != 0) // avoid connecting to self
                                    && (hex || diags || (a == 0 || b == 0))
                                    && (!hex || a == 0 || (i % 2 == 1 && b != 1) || (i % 2 == 0 && b != -1)))
                                {
                                    int ai = a + i;
                                    int bi = b + o;
                                    if (ai >= 0 && bi >= 0 && ai < width && bi < height)
                                    {
                                        if (gaps == null || !gaps[ai, bi])
                                        {
                                            float dist = 1;
                                            AddConnection(ids[i, o], ids[ai, bi], distance: dist, symmetrical: false);
                                        }
                                    }
                                }
                                b++;
                            }
                            a++;
                        }
                    }
                    o++;
                }
                i++;
            }

            // return the set of ids
            return ids;
        }

        // note: the cost method params are "from id", "to id", "distance", "from info", "to info"
        public List<int> Djikstra(int start, int end, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null, List<(float, float)> shape = null, float radius = 0.5f)
        {
            List<int> path = scratch.Path;
            path.Clear();
            List<int> next = scratch.Next;
            next.Clear();
            Dictionary<int, float> dists = scratch.Dists;
            dists.Clear();
            Dictionary<int, int> lasts = scratch.Lasts;
            lasts.Clear();

            // first, put all the nodes in the list to be checked
            foreach (int id in Nodes.Keys)
            {
                // the start node is a freebie, we can always path out of our starting square
                if (id != start) 
                { 
                    if (shape != null)
                    {
                        if (IsBlocked(id, layer, shape, radius))
                            continue;
                    }
                    else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                        continue;
                }

                dists.Add(id, float.MaxValue); // start them all at max distance, we'll set the start
                                               // node to 0 distance to force it to be first
                next.Add(id);
            }
            dists[start] = 0; // force the start node to be our first pick

            // now we iterate until we find the end
            while (next.Count > 0)
            {
                // we need to find whichever node has the shortest dist
                int cur = 0;
                float shortest = float.MaxValue;
                foreach (int id in next)
                {
                    if (dists[id] < shortest)
                    {
                        shortest = dists[id];
                        cur = id;
                    }
                }

                // now remove this node from the next list
                next.Remove(cur);

                if (shortest == float.MaxValue)
                {
                    // if our shortest was the max distance, there's no possible path
                    // so we should abort
                    break;
                }

                if (cur == end)
                {
                    // if the shortest is the end, we have the path
                    path.Clear();
                    // construct the path by iterating over our history
                    while (lasts.ContainsKey(cur))
                    {
                        path.Insert(0, cur);
                        cur = lasts[cur];
                    }
                    // ensure the start is included
                    path.Insert(0, start);
                    break;
                }

                // otherwise, we need to keep searching
                foreach (int id in Nodes[cur].Keys)
                {
                    // the start node is a freebie, we can always path out of our starting square
                    if (id != start)
                    {
                        if (shape != null)
                        {
                            if (IsBlocked(id, layer, shape, radius))
                                continue;
                        }
                        else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                            continue;
                    }

                    float nextdist = dists[cur] + Nodes[cur][id];
                    if (cost != null)
                        nextdist = cost(cur, id, nextdist, Info[cur], Info[id]);

                    if (nextdist < dists[id])
                    {
                        dists[id] = nextdist;
                        lasts[id] = cur;
                    }
                }
            }
            return path;
        }

        public Dictionary<int, int> DjikstraFlowfield(int target, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null, List<(float, float)> shape = null, float radius = 0.5f)
        {
            List<int> next = scratch.Next;
            next.Clear();
            Dictionary<int, float> dists = scratch.Dists;
            dists.Clear();
            Dictionary<int, int> lasts = scratch.Lasts;
            lasts.Clear();

            // first, put all the nodes in the list to be checked
            foreach (int id in Nodes.Keys)
            {
                if (shape != null)
                {
                    if (IsBlocked(id, layer, shape, radius))
                        continue;
                }
                else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                    continue;

                dists.Add(id, float.MaxValue); // start them all at max distance, we'll set the start
                                               // node to 0 distance to force it to be first
                next.Add(id);
            }
            dists[target] = 0; // force the start node to be our first pick

            // now we iterate until we find the end
            while (next.Count > 0)
            {
                // we need to find whichever node has the shortest dist
                int cur = 0;
                float shortest = float.MaxValue;
                foreach (int id in next)
                {
                    if (dists[id] < shortest)
                    {
                        shortest = dists[id];
                        cur = id;
                    }
                }

                // now remove this node from the next list
                next.Remove(cur);

                if (shortest == float.MaxValue)
                {
                    // if our shortest was the max distance, there's no possible path
                    // so we should abort
                    break;
                }

                foreach (int id in Nodes[cur].Keys)
                {
                    if (shape != null)
                    {
                        if (IsBlocked(id, layer, shape, radius))
                            continue;
                    }
                    else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                        continue;

                    float nextdist = dists[cur] + Nodes[cur][id];
                    if (cost != null)
                        nextdist = cost(cur, id, nextdist, Info[cur], Info[id]);

                    if (nextdist < dists[id])
                    {
                        dists[id] = nextdist;
                        lasts[id] = cur;
                    }
                }
            }
            return lasts;
        }

        public struct SearchResult
        {
            public float Distance;
            public int Next;

            public SearchResult(float dist, int next)
            {
                Distance = dist;
                Next = next;
            }
        }
        public Dictionary<int, SearchResult> DjikstraSearch(int start, float min, float max, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null, List<(float, float)> shape = null, float radius = 0.5f)
        {
            List<int> next = scratch.Next;
            next.Clear();
            Dictionary<int, float> dists = scratch.Dists;
            dists.Clear();
            Dictionary<int, SearchResult> lasts = scratch.Search;
            lasts.Clear();

            // first, put all the nodes in the list to be checked
            foreach (int id in Nodes.Keys)
            {
                // the start node is a freebie, we can always path out of our starting square
                if (id != start)
                {
                    if (shape != null)
                    {
                        if (IsBlocked(id, layer, shape, radius))
                            continue;
                    }
                    else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                        continue;
                }

                dists.Add(id, float.MaxValue); // start them all at max distance, we'll set the start
                                               // node to 0 distance to force it to be first
                next.Add(id);
            }
            dists[start] = 0; // force the start node to be our first pick

            // now we iterate until we find the end
            while (next.Count > 0)
            {
                // we need to find whichever node has the shortest dist
                int cur = 0;
                float shortest = float.MaxValue;
                foreach (int id in next)
                {
                    if (dists[id] < shortest)
                    {
                        shortest = dists[id];
                        cur = id;
                    }
                }

                // now remove this node from the next list
                next.Remove(cur);

                if (shortest == float.MaxValue)
                {
                    // if our shortest was the max distance, there's no possible path
                    // so we should abort
                    break;
                }

                if (shortest > max)
                {
                    // if our shortest was beyond the max distance, we can stop
                    break;
                }

                foreach (int id in Nodes[cur].Keys)
                {
                    // the start node is a freebie, we can always path out of our starting square
                    if (id != start)
                    {
                        if (shape != null)
                        {
                            if (IsBlocked(id, layer, shape, radius))
                                continue;
                        }
                        else if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                            continue;
                    }

                    float curdist = dists[cur];
                    float nextdist = curdist + Nodes[cur][id];
                    if (cost != null)
                        nextdist = cost(cur, id, nextdist, Info[cur], Info[id]);

                    if (nextdist < dists[id])
                    {
                        dists[id] = nextdist;
                        if (nextdist >= min && nextdist <= max)
                            lasts[id] = new SearchResult(nextdist, cur);
                    }
                }
            }
            return lasts;
        }
    }
}
