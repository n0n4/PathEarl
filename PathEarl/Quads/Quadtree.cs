using RelaStructures;

namespace PathEarlCore.Quads
{
    public class Quadtree<T> : IPoolable
    {
        private Quadhead<T> Head;
        public Quadtree<T> ParentTree;

        public float X;
        public float Y;
        public float W;
        public float H;

        public float CenterX;
        public float CenterY;

        private Quadtree<T> TopLeft;
        private Quadtree<T> TopRight;
        private Quadtree<T> BotLeft;
        private Quadtree<T> BotRight;

        public Quadentry<T>[] Entries;
        public int EntryCount { get; private set; } = 0;

        private int PoolIndex = -1;

        public Quadtree(Quadhead<T> head)
        {
            Head = head;

            Entries = new Quadentry<T>[16];
        }

        public int GetPoolIndex()
        {
            return PoolIndex;
        }

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }

        public void Clear()
        {
            // return each entity
            for (int i = 0; i < EntryCount; i++)
                Head.ReturnEntry(Entries[i]);

            EntryCount = 0;

            // clear up the leaves
            if (TopLeft != null)
            {
                Quadtree<T> topLeft = TopLeft;
                Quadtree<T> topRight = TopRight;
                Quadtree<T> botLeft = BotLeft;
                Quadtree<T> botRight = BotRight;

                TopLeft = null;
                TopRight = null;
                BotLeft = null;
                BotRight = null;

                Head.ReturnTree(topLeft);
                Head.ReturnTree(topRight);
                Head.ReturnTree(botLeft);
                Head.ReturnTree(botRight);
            }

            ParentTree = null;
        }

        public Quadentry<T> Insert(T obj, float x, float y, float w, float h)
        {
            Quadentry<T> entry = Head.GetEntry();
            entry.Data = obj;
            entry.X = x;
            entry.Y = y;
            entry.W = w;
            entry.H = h;

            return Insert(entry);
        }

        public Quadentry<T> Insert(Quadentry<T> entry)
        {
            // here's our insertion strategy:
            // 1. if this quad has no entries, don't bother creating any leaves
            //    we only create leaves when a node would hold >1 entries
            //    so if it has 0 entries, put the new node directly into this tree

            // 2. check if the object fits completely within one of our leaves
            //    if so, insert into that leaf instead
            //    note that we can't create leaves smaller than minLeafSize in w/h
            // 2b. if this is the 2nd entry, however, then do a special 
            //     consideration: check if the 1st entry should be put into a leaf,
            //     now that we're making leaves.
            //     only do this if we haven't made leaves yet

            // 3. otherwise, insert into our own entry list.
            //    we may need to expand our list if it is full.

            if (EntryCount == 0 && TopLeft == null)
            {
                // we have no entries, and we haven't subdivided, so just put this
                // entry directly into our list
                // there's no point in subdividing / being more specific before
                // we have any other entries to distinguish against
                if (EntryCount == Entries.Length)
                    ExpandEntries();
                Entries[EntryCount] = entry;
                entry.Leaf = this;
                EntryCount++;
                return entry;
            }

            // don't check our leaves if we're already below the minleafsize
            if (W > Head.MinLeafSize)
            {
                // check if it would fit into one of our leaves
                if (entry.X < CenterX && entry.Y < CenterY
                        && entry.X + entry.W < CenterX && entry.Y + entry.H < CenterY)
                {
                    // topleft
                    if (TopLeft == null)
                        Subdivide();

                    TopLeft.Insert(entry);
                    return entry;
                }
                else if (entry.X >= CenterX && entry.Y < CenterY
                    && entry.X + entry.W < X + W && entry.Y + entry.H < CenterY)
                {
                    // topright
                    if (TopRight == null)
                        Subdivide();

                    TopRight.Insert(entry);
                    return entry;
                }
                else if (entry.X < CenterX && entry.Y >= CenterY
                    && entry.X + entry.W < CenterX && entry.Y + entry.H < Y + H)
                {
                    // botleft
                    if (BotLeft == null)
                        Subdivide();

                    BotLeft.Insert(entry);
                    return entry;
                }
                else if (entry.X >= CenterX && entry.Y >= CenterY
                    && entry.X + entry.W < X + W && entry.Y + entry.H < Y + H)
                {
                    // botright
                    if (BotRight == null)
                        Subdivide();

                    BotRight.Insert(entry);
                    return entry;
                }
            }

            // if we get here, it doesn't fit into a leaf
            if (EntryCount == Entries.Length)
                ExpandEntries();
            Entries[EntryCount] = entry;
            entry.Leaf = this;
            EntryCount++;
            return entry;
        }

        public Quadtree<T> Find(float x, float y, float w, float h)
        {
            if (TopLeft == null)
                return this;

            // check if it would fit into one of our leaves
            if (x < CenterX && y < CenterY
                && x + w < CenterX && y + h < CenterY)
            {
                // topleft
                return TopLeft.Find(x, y, w, h);
            }
            else if (x >= CenterX && y < CenterY
                && x + w < X + W && y + h < CenterY)
            {
                // topright
                return TopRight.Find(x, y, w, h);
            }
            else if (x < CenterX && y >= CenterY
                && x + w < CenterX && y + h < Y + H)
            {
                // botleft
                return BotLeft.Find(x, y, w, h);
            }
            else if (x >= CenterX && y >= CenterY
                && x + w < X + W && y + h < Y + H)
            {
                // botright
                return BotRight.Find(x, y, w, h);
            }

            return this;
        }

        private void ExpandEntries()
        {
            Quadentry<T>[] newEntries = new Quadentry<T>[Entries.Length * 2];
            for (int i = 0; i < EntryCount; i++)
                newEntries[i] = Entries[i];
            Entries = newEntries;
        }

        public void RemoveEntry(Quadentry<T> entry)
        {
            // find the entry and remove it
            for (int i = 0; i < EntryCount; i++)
            {
                if (Entries[i] == entry)
                {
                    RemoveEntryAtIndex(i);
                    break;
                }
            }

            // check if we can clean up
            if (ParentTree == null || TopLeft != null)
                return;
            if (ParentTree.TopLeft.EntryCount == 0
                && ParentTree.TopRight.EntryCount == 0
                && ParentTree.BotLeft.EntryCount == 0
                && ParentTree.BotRight.EntryCount == 0)
            {
                // our parent no longer needs leaves
                ParentTree.Undivide();
            }
        }

        private void RemoveEntryAtIndex(int index)
        {
            if (EntryCount == 1)
            {
                EntryCount--;
                return;
            }

            Entries[index] = Entries[EntryCount - 1];
            EntryCount--;
        }

        public void Undivide()
        {
            Quadtree<T> topLeft = TopLeft;
            Quadtree<T> topRight = TopRight;
            Quadtree<T> botLeft = BotLeft;
            Quadtree<T> botRight = BotRight;

            TopLeft = null;
            TopRight = null;
            BotLeft = null;
            BotRight = null;

            Head.ReturnTree(topLeft);
            Head.ReturnTree(topRight);
            Head.ReturnTree(botLeft);
            Head.ReturnTree(botRight);
        }

        private void Subdivide()
        {
            TopLeft = Head.GetTree();
            TopRight = Head.GetTree();
            BotLeft = Head.GetTree();
            BotRight = Head.GetTree();

            TopLeft.ParentTree = this;
            TopLeft.X = X;
            TopLeft.Y = Y;
            TopLeft.W = W / 2f;
            TopLeft.H = H / 2f;
            TopLeft.CenterX = TopLeft.X + TopLeft.W / 2f;
            TopLeft.CenterY = TopLeft.Y + TopLeft.H / 2f;

            TopRight.ParentTree = this;
            TopRight.X = X + W / 2f;
            TopRight.Y = Y;
            TopRight.W = W / 2f;
            TopRight.H = H / 2f;
            TopRight.CenterX = TopRight.X + TopRight.W / 2f;
            TopRight.CenterY = TopRight.Y + TopRight.H / 2f;

            BotLeft.ParentTree = this;
            BotLeft.X = X;
            BotLeft.Y = Y + H / 2f;
            BotLeft.W = W / 2f;
            BotLeft.H = H / 2f;
            BotLeft.CenterX = BotLeft.X + BotLeft.W / 2f;
            BotLeft.CenterY = BotLeft.Y + BotLeft.H / 2f;

            BotRight.ParentTree = this;
            BotRight.X = X + W / 2f;
            BotRight.Y = Y + H / 2f;
            BotRight.W = W / 2f;
            BotRight.H = H / 2f;
            BotRight.CenterX = BotRight.X + BotRight.W / 2f;
            BotRight.CenterY = BotRight.Y + BotRight.H / 2f;

            // if we have entries at the time of subdivision, check if any of them
            // should be put into the leaves instead
            for (int i = EntryCount - 1; i >= 0; i--)
            {
                Quadentry<T> entry = Entries[i];
                if (entry.X < CenterX && entry.Y < CenterY
                    && entry.X + entry.W < CenterX && entry.Y + entry.H < CenterY)
                {
                    // topleft
                    TopLeft.Insert(entry);
                    RemoveEntryAtIndex(i);
                }
                else if (entry.X >= CenterX && entry.Y < CenterY
                    && entry.X + entry.W < X + W && entry.Y + entry.H < CenterY)
                {
                    // topright
                    TopRight.Insert(entry);
                    RemoveEntryAtIndex(i);
                }
                else if (entry.X < CenterX && entry.Y >= CenterY
                    && entry.X + entry.W < CenterX && entry.Y + entry.H < Y + H)
                {
                    // botleft
                    BotLeft.Insert(entry);
                    RemoveEntryAtIndex(i);
                }
                else if (entry.X >= CenterX && entry.Y >= CenterY
                    && entry.X + entry.W < X + W && entry.Y + entry.H < Y + H)
                {
                    // botright
                    BotRight.Insert(entry);
                    RemoveEntryAtIndex(i);
                }
            }
        }
    }
}
