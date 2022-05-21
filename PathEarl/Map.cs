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

        public Map(Func<T> infoSpawner)
        {
            InfoSpawner = infoSpawner;
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
        }

        public bool IsBlocked(int id, EMapLayer layer)
        {
            if (Blocks.TryGetValue(id, out EMapLayer blocking))
                return (blocking & layer) != 0;
            return false;
        }

        public void Unblock(int id, EMapLayer layer)
        {
            if (IsBlocked(id, layer))
                Blocks[id] &= ~layer;
        }

        public void Unblock(int id)
        {
            Blocks[id] = EMapLayer.None;
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
        public List<int> Djikstra(int start, int end, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null)
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
                if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                    continue; 

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
                    if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
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
            return path;
        }

        public Dictionary<int, int> DjikstraFlowfield(int target, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null)
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
                if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
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
                    if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
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
        public Dictionary<int, SearchResult> DjikstraSearch(int start, float min, float max, EMapLayer layer, MapScratch<T> scratch, Func<int, int, float, T, T, float> cost = null)
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
                if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                    continue;

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
                    if (IsBlocked(id, layer)) // skip any nodes that are blocked on this layer
                        continue;

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
