using PathEarlScout;
using PathEarlScout.Optimizers;
using PathEarlScout.Structures;
using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;
using static UT_PathEarlScout.BasicRuleBuilder;

namespace UT_PathEarlScout.TestScouts
{
    public static class StructureClusterScout
    {
        public static void AddRules(Scout<BasicTileInfo> scout)
        {
            ScoutRecycler<BasicTileInfo> recycler = scout.Recycler;
            InfoAccess<BasicTileInfo> infoAccess = scout.InfoAccess;
            scout.Optimizer = recycler.GetOptimizer(SimpleOptimizer<BasicTileInfo>.Keyword);

            Setup(recycler, infoAccess);

            Layer<BasicTileInfo> firstLayer = recycler.GetLayer();
            scout.Layers.Add(firstLayer);
            firstLayer.Name = "grass";
            AddRule(firstLayer.Rules, "free-tiles",
                IfCondition(CheckTileType("current", "void")),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(1)));
                });

            Layer<BasicTileInfo> lakeLayer = recycler.GetLayer();
            scout.Layers.Add(lakeLayer);
            lakeLayer.Name = "deserts";
            {
                Structure<BasicTileInfo> structure = recycler.GetStructure();
                lakeLayer.Structures.Add(structure);
                structure.Name = "desert-struct";
                structure.Repeats = 3;
                structure.Condition = AdjacentCondition(0, 10, IfCondition(CheckTileType("current", "desert", "!=")));

                StructureBlock<BasicTileInfo> structureBlock = recycler.GetStructureBlock();
                structure.Blocks.Add(structureBlock);
                structureBlock.Name = "desert-block";
                structureBlock.Beam = false;
                structureBlock.Cluster = true;
                structureBlock.ClusterMinPoints = 3;
                structureBlock.ClusterMaxPoints = 3;
                structureBlock.ClusterMinRadius = 3;
                structureBlock.ClusterMaxRadius = 3;

                StructureCell<BasicTileInfo> structureCell = recycler.GetStructureCell();
                structureBlock.Cells.Add(structureCell);
                structureCell.Name = "desert-cell";
                structureCell.MinRadius = 0;
                structureCell.Radius = 4;
                structureCell.Rules.Add(recycler.GetStructureRule());
                structureCell.Rules[0].Rule.Name = "desert-cell-rule";
                structureCell.Rules[0].Rule.Condition = AlwaysCondition();
                structureCell.Rules[0].Rule.Outcomes.Add(OutcomeTileType("desert", LiteralFloat(1f)));
            }

            Layer<BasicTileInfo> riverLayer = recycler.GetLayer();
            scout.Layers.Add(riverLayer);
            riverLayer.Name = "beach";
            {
                Structure<BasicTileInfo> structure = recycler.GetStructure();
                riverLayer.Structures.Add(structure);
                structure.Name = "beach-struct";
                structure.Repeats = 1;
                structure.Condition = IfCondition(CheckTileType("current", "desert"));

                StructureBlock<BasicTileInfo> structureBlock = recycler.GetStructureBlock();
                structure.Blocks.Add(structureBlock);
                structureBlock.Name = "beach-block";
                structureBlock.Beam = true;
                structureBlock.BeamMinDistance = 8;
                structureBlock.BeamWanderChance = 0.2f;
                structureBlock.BeamWanderRepeats = 2;
                structureBlock.BeamTargetCondition = IfCondition(CheckTileType("current", "desert"));
                structureBlock.Cluster = true;
                structureBlock.ClusterMinPoints = 1;
                structureBlock.ClusterMaxPoints = 3;
                structureBlock.ClusterMinRadius = 1;
                structureBlock.ClusterMaxRadius = 2;
                structureBlock.ClusterIncludeOrigin = true;


                StructureCell<BasicTileInfo> structureCell = recycler.GetStructureCell();
                structureBlock.Cells.Add(structureCell);
                structureCell.Name = "beach-cell";
                structureCell.MinRadius = 0;
                structureCell.Radius = 1;
                structureCell.Rules.Add(recycler.GetStructureRule());
                structureCell.Rules[0].Rule.Name = "beach-cell-rule";
                structureCell.Rules[0].Rule.Condition = AlwaysCondition();
                structureCell.Rules[0].Rule.Outcomes.Add(OutcomeTileType("beach", LiteralFloat(1f)));
            }
        }
    }
}
