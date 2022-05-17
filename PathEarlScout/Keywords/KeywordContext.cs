using PathEarlCore;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public class KeywordContext<T> : IPoolable where T : ITileInfo
    {
        public Map<T> Map;
        public int[] Ids = new int[32];
        public T[] Infos = new T[32];
        public int Count = 0;

        public List<int> AlreadyHitIds = new List<int>();
        public int AlreadyHitCount = 0;
        public int AlreadyHitStackCount = 0;

        private int PoolIndex;

        public void Setup(Map<T> map, int thisId, T thisInfo)
        {
            Map = map;
            Ids[0] = thisId;
            Infos[0] = thisInfo;
            Count = 1;
            AlreadyHitCount = 0;
        }

        public void Add(int id, T info)
        {
            if (Count >= Ids.Length)
                throw new Exception("Stack size too large!");
            Ids[Count] = id;
            Infos[Count] = info;
            Count++;
        }

        public void Remove()
        {
            Count--;
        }

        public void EndRule()
        {
            // between rules, already-hits no longer apply
            AlreadyHitCount = 0;
        }

        public void AddAlreadyHitStack()
        {
            AlreadyHitStackCount++;
        }

        public void RemoveAlreadyHitStack()
        {
            AlreadyHitStackCount--;
            if (AlreadyHitStackCount <= 0)
            {
                AlreadyHitStackCount = 0;
                AlreadyHitCount = 0;
            }
        }

        public void AddHit(int id)
        {
            if (AlreadyHitCount < AlreadyHitIds.Count)
                AlreadyHitIds[AlreadyHitCount] = id;
            else
                AlreadyHitIds.Add(id);
            AlreadyHitCount++;
        }

        public bool IsAlreadyHit(int id)
        {
            for (int i = 0; i < AlreadyHitCount; i++)
                if (AlreadyHitIds[i] == id)
                    return true;
            return false;
        }

        public int GetIndex(string owner)
        {
            if (owner == "root")
                return 0;
            else if (owner == "current")
                return Count - 1;
            else if (owner == "previous")
                return Count - 2;
            else if (owner.StartsWith("back-"))
            {
                string remainder = owner.Substring(5);
                if (!int.TryParse(remainder, out int back))
                    throw new Exception("Could not parse the number in 'back-" + remainder + "'");
                return (Count - 1) - back;
            }
            else
            {
                if (!int.TryParse(owner, out int index))
                    throw new Exception("Could not parse the number '" + owner + "' as a keyword owner");
                return index;
            }

            throw new Exception("Unrecognized keyword owner '" + owner + "'");
        }

        public T GetInfo(string owner)
        {
            return Infos[GetIndex(owner)];
        }

        public int GetId(string owner)
        {
            return Ids[GetIndex(owner)];
        }

        public void Clear()
        {
            Count = 0;
            AlreadyHitCount = 0;
        }

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }

        public int GetPoolIndex()
        {
            return PoolIndex;
        }
    }
}
