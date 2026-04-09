using PathEarlCore;

namespace PathEarlScout.Structures
{
    public class StructureRule<T> where T : ITileInfo
    {
        public int? MinRadius = null;
        public int? Radius = null;
        public float? MinXOffset = null;
        public float? MaxXOffset = null;
        public float? MinYOffset = null;
        public float? MaxYOffset = null;

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
