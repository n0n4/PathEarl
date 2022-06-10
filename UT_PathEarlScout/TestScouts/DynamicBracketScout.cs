using PathEarlScout;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;
using System.Text;

using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;
using static UT_PathEarlScout.BasicRuleBuilder;

namespace UT_PathEarlScout.TestScouts
{
    public static class DynamicBracketScout
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
            AddRule(firstLayer.GlobalRules, "setup",
                AlwaysCondition(),
                (list) =>
                {
                    list.Add(SimpleOutcomeKeywordString("strings", "biome0", "swamp"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome1", "swamp"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome2", "desert"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome3", "desert"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome4", "desert"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome5", "desert"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome6", "swamp"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome7", "swamp"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome8", "swamp"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome9", "desert"));
                    list.Add(SimpleOutcomeKeywordString("strings", "biome10", "desert"));
                });

            AddRule(firstLayer.Rules, "convert",
                IfCondition(Check(
                    null, null, KeywordInt("current", "id"),
                    "<=", 
                    null, null, LiteralInt(10))),
                (list) => {
                    list.Add(OutcomeString(
                        LiteralString("current"), LiteralString("type"),
                        "=",
                        null, KeywordString(LiteralString("strings"), KeywordString(null, null, "biome", "+", null, KeywordInt("current", "id"))), null));
                });
        }
    }
}
