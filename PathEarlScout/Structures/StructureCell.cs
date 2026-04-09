using PathEarlCore;
using PathEarlScout.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Structures
{
    public class StructureCell<T> where T : ITileInfo
    {
        public string Name = string.Empty;

        public int MinRadius = 0;
        public int Radius = 0;
        public float MinXOffset = float.MinValue;
        public float MaxXOffset = float.MaxValue;
        public float MinYOffset = float.MinValue;
        public float MaxYOffset = float.MaxValue;
        // e.g. could make a square by defining a large radius and then setting a min/max xy offset
        // that is a square within that radius

        public List<StructureRule<T>> Rules = new List<StructureRule<T>>();
        public StructureCell(ScoutRecycler<T> recycler)
        {
            Rules = recycler.GetStructureRuleList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            recycler.ReturnStructureRuleList(Rules);
        }
    }
}
