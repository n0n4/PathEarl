using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore.Quads
{
    public class Quadchecker<T, TChecker>
        where TChecker : struct, IQuadCollisionChecker<T>
    {
        private readonly TChecker Checker = new TChecker();

        public Quadhead<T> Head;

        public Quadchecker(Quadhead<T> head)
        {
            Head = head;
        }

        // check at current position
        public int Check(Quadentry<T> entry, List<Quadentry<T>> results)
        {
            return Check(entry, entry.Leaf, results);
        }

        // check at a supposed position (more useful for movement)
        public int Check(Quadentry<T> entry, List<Quadentry<T>> results,
            float x, float y, float w, float h)
        {
            Quadtree<T> tree = Head.Find(x, y, w, h);

            float ox = entry.X;
            float oy = entry.Y;
            float ow = entry.W;
            float oh = entry.H;
            entry.X = x;
            entry.Y = y;
            entry.W = w;
            entry.H = h;
            int count = Check(entry, tree, results);
            entry.X = ox;
            entry.Y = oy;
            entry.W = ow;
            entry.H = oh;
            return count;
        }

        private int Check(Quadentry<T> entry, Quadtree<T> tree, List<Quadentry<T>> results)
        {
            int count = 0;
            for (int i = 0; i < tree.EntryCount; i++)
            {
                Quadentry<T> target = tree.Entries[i];

                // can't collide with ourselves
                if (target == entry)
                    continue;

                // check if these two collide
                if (Checker.Collides(entry, target))
                {
                    results.Add(target);
                    count++;
                }
            }

            // if the parent exists, check its entries as well
            if (tree.ParentTree != null)
            {
                count += Check(entry, tree.ParentTree, results);
            }

            return count;
        }

        // check at current position, stop after first found
        public Quadentry<T> CheckFirst(Quadentry<T> entry)
        {
            return CheckFirst(entry, entry.Leaf);
        }

        // check at a supposed position (more useful for movement)
        // stop after first found
        public Quadentry<T> CheckFirst(Quadentry<T> entry,
            float x, float y, float w, float h)
        {
            Quadtree<T> tree = Head.Find(x, y, w, h);

            float ox = entry.X;
            float oy = entry.Y;
            float ow = entry.W;
            float oh = entry.H;
            entry.X = x;
            entry.Y = y;
            entry.W = w;
            entry.H = h;
            Quadentry<T> found = CheckFirst(entry, tree);
            entry.X = ox;
            entry.Y = oy;
            entry.W = ow;
            entry.H = oh;
            return found;
        }

        private Quadentry<T> CheckFirst(Quadentry<T> entry, Quadtree<T> tree)
        {
            for (int i = 0; i < tree.EntryCount; i++)
            {
                Quadentry<T> target = tree.Entries[i];

                // can't collide with ourselves
                if (target == entry)
                    continue;

                // check if these two collide
                if (Checker.Collides(entry, target))
                {
                    return target;
                }
            }

            // if the parent exists, check its entries as well
            if (tree.ParentTree != null)
            {
                return CheckFirst(entry, tree.ParentTree);
            }

            return null;
        }
    }
}
