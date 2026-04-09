using PathEarlScout;
using PathEarlScout.Optimizers;
using PathEarlScout.Structures;
using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;
using static UT_PathEarlScout.BasicRuleBuilder;

namespace UT_PathEarlScout.TestScouts
{
    public static class StructureScout
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
            lakeLayer.Name = "lakes";
            {
                Structure<BasicTileInfo> structure = recycler.GetStructure();
                lakeLayer.Structures.Add(structure);
                structure.Name = "lake-struct";
                structure.Repeats = 3;
                structure.Condition = AdjacentCondition(0, 10, IfCondition(CheckTileType("current", "water", "!=")));

                StructureBlock<BasicTileInfo> structureBlock = recycler.GetStructureBlock();
                structure.Blocks.Add(structureBlock);
                structureBlock.Name = "lake-block";
                structureBlock.Beam = false;
                
                StructureCell<BasicTileInfo> structureCell = recycler.GetStructureCell();
                structureBlock.Cells.Add(structureCell);
                structureCell.Name = "lake-cell";
                structureCell.MinRadius = 0;
                structureCell.Radius = 4;
                structureCell.Rules.Add(recycler.GetStructureRule());
                structureCell.Rules[0].Rule.Name = "lake-cell-rule";
                structureCell.Rules[0].Rule.Condition = AlwaysCondition();
                structureCell.Rules[0].Rule.Outcomes.Add(OutcomeTileType("water", LiteralFloat(1f)));
            }

            Layer<BasicTileInfo> riverLayer = recycler.GetLayer();
            scout.Layers.Add(riverLayer);
            riverLayer.Name = "rivers";
            {
                Structure<BasicTileInfo> structure = recycler.GetStructure();
                riverLayer.Structures.Add(structure);
                structure.Name = "river-struct";
                structure.Repeats = 1;
                structure.Condition = IfCondition(CheckTileType("current", "water"));

                StructureBlock<BasicTileInfo> structureBlock = recycler.GetStructureBlock();
                structure.Blocks.Add(structureBlock);
                structureBlock.Name = "river-block";
                structureBlock.Beam = true;
                structureBlock.BeamMinDistance = 8;
                structureBlock.BeamWanderChance = 0.2f;
                structureBlock.BeamWanderRepeats = 2;
                structureBlock.BeamTargetCondition = IfCondition(CheckTileType("current", "water"));

                StructureCell<BasicTileInfo> structureCell = recycler.GetStructureCell();
                structureBlock.Cells.Add(structureCell);
                structureCell.Name = "river-cell";
                structureCell.MinRadius = 0;
                structureCell.Radius = 1;
                structureCell.Rules.Add(recycler.GetStructureRule());
                structureCell.Rules[0].Rule.Name = "river-cell-rule";
                structureCell.Rules[0].Rule.Condition = AlwaysCondition();
                structureCell.Rules[0].Rule.Outcomes.Add(OutcomeTileType("river", LiteralFloat(1f)));
            }
        }
    }
}
