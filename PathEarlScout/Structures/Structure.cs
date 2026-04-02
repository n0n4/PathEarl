using PathEarlCore;
using PathEarlScout.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Structures
{
    public class Structure<T> where T : ITileInfo
    {
        // idea is basically
        // 1. point selection criteria
        // - how many
        // - any conditions
        // 2. structure placement
        // - composed of blocks
        // - each block has a shape (or is a path)

        public string Name;
        public int MinPoints = 1;
        public int MaxPoints = 1;
        public double Rarity = 1;
        public int Repeats = 0;
        public ICondition<T> Condition = null;

        public List<StructureBlock<T>> Blocks = new List<StructureBlock<T>>();

        public Structure(ScoutRecycler<T> recycler)
        {
            Blocks = recycler.GetStructureBlockList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach (StructureBlock<T> block in Blocks)
                recycler.ReturnStructureBlock(block);
            Blocks.Clear();

            if (Condition != null)
            {
                recycler.ReturnCondition(Condition);
                Condition = null;
            }
        }
    }
}
