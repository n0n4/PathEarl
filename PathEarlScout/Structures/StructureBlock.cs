using PathEarlCore;
using PathEarlScout.Conditions;
using System.Collections.Generic;

namespace PathEarlScout.Structures
{
    public class StructureBlock<T> where T : ITileInfo
    {
        public const string BEAM_PATHFINDING_SIMPLE = "simple";
        public const string BEAM_PATHFINDING_EXPENSIVE = "expensive";

        public string Name = string.Empty;

        public bool Beam = false;
        public int BeamMinDistance = 0;
        public int BeamMaxDistance = 0;
        public ICondition<T> BeamTargetCondition = null;
        public int BeamInterval = 1;
        public string BeamPathfinding = BEAM_PATHFINDING_SIMPLE;
        public double BeamWanderChance = 0;
        public int BeamWanderRepeats = 0; // how many extra times can it wander at once?
        public ICondition<T> BeamWanderCondition = null;

        public bool Cluster = false;
        public int ClusterMinPoints = 0;
        public int ClusterMaxPoints = 0;
        public int ClusterMinRadius = 0;
        public int ClusterMaxRadius = 0;
        public ICondition<T> ClusterCondition = null;

        public List<StructureCell<T>> Cells;

        public StructureBlock(ScoutRecycler<T> recycler)
        {
            Cells = recycler.GetStructureCellList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach (StructureCell<T> cell in Cells)
                recycler.ReturnStructureCell(cell);
            Cells.Clear();

            if (BeamTargetCondition != null)
            {
                recycler.ReturnCondition(BeamTargetCondition);
                BeamTargetCondition = null;
            }

            if (BeamWanderCondition != null)
            {
                recycler.ReturnCondition(BeamWanderCondition);
                BeamWanderCondition = null;
            }

            if (ClusterCondition != null)
            {
                recycler.ReturnCondition(ClusterCondition);
                ClusterCondition = null;
            }
        }
    }
}
