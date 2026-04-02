using PathEarlCore;

namespace PathEarlScout.Structures
{
    public class StructureRule<T> where T : ITileInfo
    {
        public int MinRadius = 0;
        public int Radius = 0;
        public float MinXOffset = float.MinValue;
        public float MaxXOffset = float.MaxValue;
        public float MinYOffset = float.MinValue;
        public float MaxYOffset = float.MaxValue;

        public Rule<T> Rule;
        public StructureRule(ScoutRecycler<T> recycler)
        {
            Rule = recycler.GetRule();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            recycler.ReturnRule(Rule);
        }
    }
}
