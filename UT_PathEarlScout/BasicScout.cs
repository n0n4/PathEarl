using PathEarlCore;
using PathEarlScout;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;
using System.Text;

namespace UT_PathEarlScout
{
    public static class BasicScout
    {
        public static ScoutRecycler<BasicTileInfo> Recycler = new ScoutRecycler<BasicTileInfo>();
        public static MapScratch<BasicTileInfo> MapScratch = new MapScratch<BasicTileInfo>();
        public static InfoAccess<BasicTileInfo> InfoAccess = BasicInfoAccess.GetInfoAccess();

        public static Scout<BasicTileInfo> MakeScout(Map<BasicTileInfo> map, string name)
        {
            Scout<BasicTileInfo> scout = new Scout<BasicTileInfo>(map, MapScratch, Recycler, InfoAccess);
            scout.Name = name;
            return scout;
        }

        public static Map<BasicTileInfo> MakeHexMap(int length, int height, out BasicTileInfo[,] infos)
        {
            Map<BasicTileInfo> map = new Map<BasicTileInfo>(() => { return new BasicTileInfo(); });
            map.GenerateGrid(length, height, out infos, null, hex: true);
            return map;
        }

        public static void AddBasicScoutRules(Scout<BasicTileInfo> scout)
        {
            scout.Optimizer = Recycler.GetOptimizer(SimpleOptimizer<BasicTileInfo>.Keyword);

            AddRule(scout.GlobalRules, "deep-water",
                AdjacentCondition(6, -1,
                    OrCondition((or) => {
                        or.Add(IfCondition(CheckTileType("current", "ocean")));
                        or.Add(IfCondition(CheckTileType("current", "water")));
                    })
                ),
                (list) => {
                    list.Add(OutcomeTileType("ocean", 3));
                });

            Layer<BasicTileInfo> firstLayer = Recycler.GetLayer();
            scout.Layers.Add(firstLayer);
            firstLayer.Name = "types";
            firstLayer.Repeats = 2;
            AddRule(firstLayer.Rules, "free-tiles",
                IfCondition(CheckTileType("current", "void")),
                (list) => {
                    list.Add(OutcomeTileType("grass", 2));
                    list.Add(OutcomeTileType("dirt", 0.6f));
                    list.Add(OutcomeTileType("desert", 0.2f));
                    list.Add(OutcomeTileType("swamp", 0.2f));
                    list.Add(OutcomeTileType("water", 0.1f));
                });

            AddRule(firstLayer.Rules, "stay-dirt",
                IfCondition(CheckTileType("current", "dirt")),
                (list) => {
                    list.Add(OutcomeTileType("dirt", 1f));
                });

            AddRule(firstLayer.Rules, "stay-grass",
                IfCondition(CheckTileType("current", "grass")),
                (list) => {
                    list.Add(OutcomeTileType("grass", 2f));
                });

            AddRule(firstLayer.Rules, "stay-ocean",
                IfCondition(CheckTileType("current", "ocean")),
                (list) => {
                    list.Add(OutcomeTileType("ocean", 20f));
                });

            AddRule(firstLayer.Rules, "stay-water",
                IfCondition(CheckTileType("current", "water")),
                (list) => {
                    list.Add(OutcomeTileType("water", 2f));
                });

            AddRule(firstLayer.Rules, "stay-desert",
                IfCondition(CheckTileType("current", "desert")),
                (list) => {
                    list.Add(OutcomeTileType("desert", 1f));
                });

            AddRule(firstLayer.Rules, "stay-swamp",
                IfCondition(CheckTileType("current", "swamp")),
                (list) => {
                    list.Add(OutcomeTileType("swamp", 1f));
                });

            AddRule(firstLayer.Rules, "preserve-clump",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileType("current", "void", "!=")));
                    list.Add(AdjacentCondition(3, -1,
                        IfCondition(CompareTileType("current", "back-1"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileTypeByKeyword("current", "type", 2.5f));
                });

            AddRule(firstLayer.Rules, "evaporate",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileArchetype("current", "water")));
                    list.Add(AdjacentCondition(0, 0,
                        IfCondition(CheckTileArchetype("current", "water"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileType("dirt", 2.5f));
                    list.Add(OutcomeTileType("grass", 2.5f));
                });

            AddRule(firstLayer.Rules, "more-grass",
                AdjacentCondition(1, -1,
                    IfCondition(CheckTileType("current", "grass"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", 0.5f));
                });

            AddRule(firstLayer.Rules, "clump-grass",
                AdjacentCondition(3, 5,
                    IfCondition(CheckTileType("current", "grass"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", 1.5f));
                });

            AddRule(firstLayer.Rules, "more-desert",
                AdjacentCondition(1, -1,
                    IfCondition(CheckTileType("current", "desert"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", 1.5f));
                    list.Add(OutcomeTileType("dirt", 0.5f));
                });

            AddRule(firstLayer.Rules, "even-more-desert",
                AdjacentCondition(3, -1,
                    IfCondition(CheckTileType("current", "desert"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", 1.5f));
                });

            AddRule(firstLayer.Rules, "far-desert",
                AdjacentCondition(1, -1,
                    AdjacentCondition(1, -1,
                        IfCondition(CheckTileType("current", "desert"))
                    )
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", 0.2f));
                });

            AddRule(firstLayer.Rules, "more-water",
                AdjacentCondition(2, -1, 0, 2,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", 1.5f));
                });

            AddRule(firstLayer.Rules, "clump-water",
                AdjacentCondition(6, -1, 0, 3,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", 3.5f));
                });

            AddRule(firstLayer.Rules, "flood-water",
                AdjacentCondition(5, -1,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", 3.5f));
                });

            AddRule(firstLayer.Rules, "start-river",
                AdjacentCondition(2, -1,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("river", 0.25f));
                });

            AddRule(firstLayer.Rules, "shallows-beaches",
                AndCondition((list) => {
                    list.Add(AdjacentCondition(1, -1,
                        IfCondition(CheckTileArchetype("current", "water"))
                    ));
                    list.Add(AdjacentCondition(1, -1,
                        IfCondition(CheckTileArchetype("current", "ground"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileType("shallows", 0.5f));
                    list.Add(OutcomeTileType("beach", 0.5f));
                });

            AddRule(firstLayer.Rules, "swamp-growth",
                AdjacentCondition(1, -1,
                    AdjacentCondition(1, -1,
                        IfCondition(CheckTileType("current", "swamp"))
                    )
                ),
                (list) => {
                    list.Add(OutcomeTileType("swamp", 0.5f));
                    list.Add(OutcomeTileType("shallows", 0.1f));
                });

            AddRule(firstLayer.Rules, "swamp-growth",
                AdjacentCondition(4, -1,
                    OrCondition((list) => {
                        list.Add(IfCondition(CheckTileType("current", "swamp")));
                        list.Add(IfCondition(CheckTileType("current", "shallows")));
                        list.Add(IfCondition(CheckTileType("current", "water")));
                    })
                ),
                (list) => {
                    list.Add(OutcomeTileType("swamp", 1.5f));
                    list.Add(OutcomeTileType("shallows", 0.5f));
                });

            AddRule(firstLayer.Rules, "swamp-harden",
                AdjacentCondition(5, -1,
                    OrCondition((list) => {
                        list.Add(IfCondition(CheckTileType("current", "swamp")));
                        list.Add(IfCondition(CheckTileType("current", "shallows")));
                    })
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", 1.5f));
                    list.Add(OutcomeTileType("dirt", 1.5f));
                });

            Layer<BasicTileInfo> riverLayer = Recycler.GetLayer();
            scout.Layers.Add(riverLayer);
            riverLayer.Name = "rivers";
            riverLayer.Repeats = 20;
            AddRule(riverLayer.Rules, "continue-river",
                AndCondition((and) => {
                    and.Add(IfCondition(CheckTileArchetype("current", "ground")));
                    and.Add(AdjacentCondition(1, 1,
                        IfCondition(CheckTileType("current", "river"))
                    ));
                    and.Add(AdjacentCondition(0, 2, 0, 2,
                            IfCondition(CheckTileType("current", "river"))
                    ));
                    and.Add(AdjacentCondition(4, -1,
                        IfCondition(CheckTileArchetype("current", "ground"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileType("river", 100f));
                });

            Layer<BasicTileInfo> secondLayer = Recycler.GetLayer();
            scout.Layers.Add(secondLayer);
            secondLayer.Name = "height";

            AddRule(secondLayer.Rules, "free-height",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileArchetype("current", "ground")));
                    list.Add(NotCondition((not) => {
                        not.Add(IfCondition(CheckTileType("current", "swamp")));
                        not.Add(IfCondition(CheckTileType("current", "beach")));
                        not.Add(IfCondition(CheckTileType("current", "ruins")));
                        not.Add(IfCondition(CheckTileType("current", "gate")));
                        not.Add(IfCondition(CheckTileType("current", "walls")));
                    }));
                }),
                (list) => {
                    list.Add(OutcomeTileHeight("flat", 1f));
                    list.Add(OutcomeTileHeight("hills", 0.3f));
                    list.Add(OutcomeTileHeight("mountains", 0.1f));
                });

            Layer<BasicTileInfo> thirdLayer = Recycler.GetLayer();
            scout.Layers.Add(thirdLayer);
            thirdLayer.Name = "features";

            AddRule(thirdLayer.Rules, "free-features",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileArchetype("current", "ground")));
                    list.Add(NotCondition((not) => {
                        not.Add(IfCondition(CheckTileType("current", "desert")));
                        not.Add(IfCondition(CheckTileType("current", "dirt")));
                        not.Add(IfCondition(CheckTileType("current", "beach")));
                    }));
                }),
                (list) => {
                    list.Add(OutcomeTileFeature("empty", 1f));
                    list.Add(OutcomeTileFeature("forest", 0.3f));
                });
        }

        private static Rule<BasicTileInfo> AddRule(List<Rule<BasicTileInfo>> rules,
            string name, ICondition<BasicTileInfo> condition, Action<List<Outcome<BasicTileInfo>>> addOutcomes)
        {
            Rule<BasicTileInfo> rule = new Rule<BasicTileInfo>(Recycler);
            rules.Add(rule);
            rule.Name = name;
            rule.Condition = condition;
            addOutcomes(rule.Outcomes);
            return rule;
        }

        private static ConditionAnd<BasicTileInfo> AndCondition(Action<List<ICondition<BasicTileInfo>>> addConditions)
        {
            ConditionAnd<BasicTileInfo> cond = new ConditionAnd<BasicTileInfo>(Recycler);
            addConditions(cond.Conditions);
            return cond;
        }

        private static ConditionOr<BasicTileInfo> OrCondition(Action<List<ICondition<BasicTileInfo>>> addConditions)
        {
            ConditionOr<BasicTileInfo> cond = new ConditionOr<BasicTileInfo>(Recycler);
            addConditions(cond.Conditions);
            return cond;
        }

        private static ConditionNot<BasicTileInfo> NotCondition(Action<List<ICondition<BasicTileInfo>>> addConditions)
        {
            ConditionNot<BasicTileInfo> cond = new ConditionNot<BasicTileInfo>(Recycler);
            addConditions(cond.Conditions);
            return cond;
        }

        private static ConditionAdjacent<BasicTileInfo> AdjacentCondition(int min, int max, ICondition<BasicTileInfo> condition)
        {
            ConditionAdjacent<BasicTileInfo> cond = new ConditionAdjacent<BasicTileInfo>(Recycler);
            cond.Min = min;
            cond.Max = max;
            cond.Condition = condition;
            return cond;
        }

        private static ConditionAdjacent<BasicTileInfo> AdjacentCondition(int min, int max, int minRange, int maxRange, ICondition<BasicTileInfo> condition)
        {
            ConditionAdjacent<BasicTileInfo> cond = new ConditionAdjacent<BasicTileInfo>(Recycler);
            cond.Min = min;
            cond.Max = max;
            cond.StartRange = minRange;
            cond.StopRange = maxRange;
            cond.Condition = condition;
            return cond;
        }

        private static ConditionIf<BasicTileInfo> IfCondition(ScoutCheck<BasicTileInfo> check)
        {
            ConditionIf<BasicTileInfo> cond = new ConditionIf<BasicTileInfo>(Recycler);
            cond.Check = check;
            return cond;
        }

        private static ScoutCheck<BasicTileInfo> Check(
            KeywordFloat<BasicTileInfo> firstFloat, KeywordString<BasicTileInfo> firstString, KeywordInt<BasicTileInfo> firstInt,
            string operation,
            KeywordFloat<BasicTileInfo> secondFloat, KeywordString<BasicTileInfo> secondString, KeywordInt<BasicTileInfo> secondInt)
        {
            ScoutCheck<BasicTileInfo> check = Recycler.ScoutCheckPool.Request();
            check.FirstKeywordFloat = firstFloat;
            check.FirstKeywordInt = firstInt;
            check.FirstKeywordString = firstString;
            check.Operation = operation;
            check.SecondKeywordFloat = secondFloat;
            check.SecondKeywordInt = secondInt;
            check.SecondKeywordString = secondString;

            return check;
        }

        private static ScoutCheck<BasicTileInfo> CheckTileType(string owner, string type, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "type"), null, comparison, null, KeywordString(null, null, type), null);
        }

        private static ScoutCheck<BasicTileInfo> CompareTileType(string owner, string otherOwner, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "type"), null, comparison, null, KeywordString(otherOwner, "type", null), null);
        }

        private static ScoutCheck<BasicTileInfo> CheckTileArchetype(string owner, string type, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "archetype"), null, comparison, null, KeywordString(null, null, type), null);
        }

        private static KeywordFloat<BasicTileInfo> KeywordFloat(string owner, string keyword, float literal = 0, string nextop = null, KeywordFloat<BasicTileInfo> next = null)
        {
            KeywordFloat<BasicTileInfo> key = Recycler.KeywordFloatPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null)
                InfoAccess.GetFloats.TryGetValue(key.Keyword, out key.Accessor);

            key.HasNext = next != null;
            key.NextFloat = next;
            key.NextOperation = nextop;
            return key;
        }

        private static KeywordInt<BasicTileInfo> KeywordInt(string owner, string keyword, int literal = 0, string nextop = null, KeywordInt<BasicTileInfo> next = null)
        {
            KeywordInt<BasicTileInfo> key = Recycler.KeywordIntPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null)
                InfoAccess.GetInts.TryGetValue(key.Keyword, out key.Accessor);

            key.HasNext = next != null;
            key.NextInt = next;
            key.NextOperation = nextop;
            return key;
        }

        private static KeywordString<BasicTileInfo> KeywordString(string owner, string keyword, string literal = null, string nextop = null, KeywordString<BasicTileInfo> next = null)
        {
            KeywordString<BasicTileInfo> key = Recycler.KeywordStringPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null)
                InfoAccess.GetStrings.TryGetValue(key.Keyword, out key.Accessor);

            key.HasNext = next != null;
            key.NextString = next;
            key.NextOperation = nextop;
            return key;
        }

        private static Outcome<BasicTileInfo> Outcome(KeywordString<BasicTileInfo> keyword, string operation,
            KeywordFloat<BasicTileInfo> keywordFloat, KeywordString<BasicTileInfo> keywordString,
            KeywordInt<BasicTileInfo> keywordInt, float probability = 1)
        {
            Outcome<BasicTileInfo> outcome = Recycler.GetOutcome();
            outcome.Keyword = keyword;
            outcome.Operation = operation;
            outcome.ValueFloat = keywordFloat;
            outcome.ValueInt = keywordInt;
            outcome.ValueString = keywordString;
            outcome.Probability = probability;
            return outcome;
        }

        private static Outcome<BasicTileInfo> OutcomeTileType(string type, float prob = 1)
        {
            return Outcome(KeywordString(null, null, "type"), "=", null, KeywordString(null, null, type), null, prob);
        }

        private static Outcome<BasicTileInfo> OutcomeTileTypeByKeyword(string owner, string keyword, float prob = 1)
        {
            return Outcome(KeywordString(null, null, "type"), "=", null, KeywordString(owner, keyword, null), null, prob);
        }

        private static Outcome<BasicTileInfo> OutcomeTileFeature(string feature, float prob = 1)
        {
            return Outcome(KeywordString(null, null, "feature"), "=", null, KeywordString(null, null, feature), null, prob);
        }

        private static Outcome<BasicTileInfo> OutcomeTileHeight(string height, float prob = 1)
        {
            return Outcome(KeywordString(null, null, "height"), "=", null, KeywordString(null, null, height), null, prob);
        }
    }
}
