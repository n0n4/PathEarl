using PathEarlScout;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;
using System.Text;

using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;
using static UT_PathEarlScout.BasicRuleBuilder;

namespace UT_PathEarlScout.TestScouts
{
    public static class GlobalVarScout
    {
        public static void AddRules(Scout<BasicTileInfo> scout)
        {
            ScoutRecycler<BasicTileInfo> recycler = scout.Recycler;
            InfoAccess<BasicTileInfo> infoAccess = scout.InfoAccess;
            scout.Optimizer = recycler.GetOptimizer(SimpleOptimizer<BasicTileInfo>.Keyword);

            Setup(recycler, infoAccess);

            Layer<BasicTileInfo> firstLayer = recycler.GetLayer();
            scout.Layers.Add(firstLayer);
            firstLayer.Name = "base";
            AddRule(firstLayer.Rules, "free-tiles",
                IfCondition(CheckTileType("current", "void")),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(2)));
                    list.Add(OutcomeTileType("dirt", LiteralFloat(0.6f)));
                    list.Add(OutcomeTileType("desert", LiteralFloat(0.2f)));
                    list.Add(OutcomeTileType("swamp", LiteralFloat(0.2f)));
                    list.Add(OutcomeTileType("water", LiteralFloat(0.1f)));
                });

            Layer<BasicTileInfo> secondLayer = recycler.GetLayer();
            scout.Layers.Add(secondLayer);
            secondLayer.Name = "dry-up";
            AddRule(secondLayer.AutoRules, "check-tiles",
                IfCondition(CheckTileType("current", "desert")),
                (list) => {
                    list.Add(OutcomeInt(LiteralString("ints"), LiteralString("dust"),
                        "+", null, null, LiteralInt(1)));
                });

            AddRule(secondLayer.Rules, "get-dry",
                AndCondition((and) => {
                    and.Add(IfCondition(CheckGlobalInt("dust", 20, ">=")));
                    and.Add(NotCondition(IfCondition(CheckTileType("current", "desert"))));
                }),
                (list) => {
                    list.Add(OutcomeTileType("desert", LiteralFloat(0.5f)));
                    list.Add(OutcomeTileType("dirt", LiteralFloat(0.5f)));
                });
        }
    }
}
