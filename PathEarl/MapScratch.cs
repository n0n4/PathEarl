using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore
{
    // stores reusable datastructures that the pathfinding algo needs
    // you should keep one MapScratch per thread and reuse it rather
    // than instantiating new ones every time
    public class MapScratch<T> where T : ITileInfo
    {
        public List<int> Path = new List<int>();
        public List<int> Next = new List<int>();
        public Dictionary<int, float> Dists = new Dictionary<int, float>();
        public Dictionary<int, int> Lasts = new Dictionary<int, int>();
        public Dictionary<int, Map<T>.SearchResult> Search = new Dictionary<int, Map<T>.SearchResult>();
    }
}
