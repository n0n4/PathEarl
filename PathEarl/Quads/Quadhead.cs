using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore.Quads
{
    public class Quadhead<T>
    {
        private ReArrayIdPool<Quadtree<T>> TreePool;
        private ReArrayIdPool<Quadentry<T>> EntryPool;

        public Quadtree<T> Tree;
        public float MinLeafSize;
        private T DefaultT;

        // note: minLeafSize is an important parameter; we won't create any 
        // subquads whose bounds are smaller than this length. This avoids the
        // problem of the quadtree becoming too granular.
        public Quadhead(T defaultT,
            float minLeafSize,
            int maxLeaves = 2500,
            int maxEntries = 10000)
        {
            MinLeafSize = minLeafSize;
            DefaultT = defaultT;

            TreePool = new ReArrayIdPool<Quadtree<T>>(32, maxLeaves,
                () => { return new Quadtree<T>(this); },
                (q) => { q.Clear(); });

            EntryPool = new ReArrayIdPool<Quadentry<T>>(32, maxEntries,
                () => { return new Quadentry<T>(DefaultT); },
                (e) => { e.Clear(); });
        }

        public void StartTree(float x, float y, float w, float h)
        {
            Tree = GetTree();
            Tree.X = x;
            Tree.Y = y;
            Tree.W = w;
            Tree.H = h;
        }

        public void ReturnTree(Quadtree<T> tree)
        {
            TreePool.Return(tree);
        }

        public Quadtree<T> GetTree()
        {
            return TreePool.Request();
        }

        public void ReturnEntry(Quadentry<T> entry)
        {
            EntryPool.Return(entry);
        }

        public Quadentry<T> GetEntry()
        {
            return EntryPool.Request();
        }

        public Quadentry<T> Insert(T obj, float x, float y, float w, float h)
        {
            return Tree.Insert(obj, x, y, w, h);
        }

        public void Remove(Quadentry<T> entry)
        {
            entry.Leaf.RemoveEntry(entry);
            ReturnEntry(entry);
        }

        public void Move(Quadentry<T> entry, float x, float y, float w, float h)
        {
            entry.Leaf.RemoveEntry(entry);
            entry.X = x;
            entry.Y = y;
            entry.W = w;
            entry.H = h;
            Tree.Insert(entry);
        }

        public void Clear()
        {
            if (Tree != null)
            {
                ReturnTree(Tree);
                Tree = null;
            }
        }

        // finds the quadtree that a certain rect belongs in
        public Quadtree<T> Find(float x, float y, float w, float h)
        {
            return Tree.Find(x, y, w, h);
        }
    }
}
