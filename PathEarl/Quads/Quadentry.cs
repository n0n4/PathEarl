using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore.Quads
{
    public class Quadentry<T> : IPoolable
    {
        public T Data;
        public float X;
        public float Y;
        public float W;
        public float H;

        private T DefaultT;
        private int PoolId = -1;

        public Quadtree<T> Leaf; // what leaf is this in?

        public Quadentry(T defaultT)
        {
            DefaultT = defaultT;
        }

        public int GetPoolIndex()
        {
            return PoolId;
        }

        public void SetPoolIndex(int index)
        {
            PoolId = index;
        }

        public void Clear()
        {
            Data = DefaultT;
            X = 0;
            Y = 0;
            W = 0;
            H = 0;
            Leaf = null;
        }
    }
}
