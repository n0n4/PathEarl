using PathEarlScout;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;
using System.Text;

using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;
using static UT_PathEarlScout.BasicRuleBuilder;

namespace UT_PathEarlScout.TestScouts
{
    public static class StandardMapScout
    {
        public static void AddRules(Scout<BasicTileInfo> scout)
        {
            ScoutRecycler<BasicTileInfo> recycler = scout.Recycler;
            InfoAccess<BasicTileInfo> infoAccess = scout.InfoAccess;
            scout.Optimizer = recycler.GetOptimizer(SimpleOptimizer<BasicTileInfo>.Keyword);

            Setup(recycler, infoAccess);

            AddRule(scout.GlobalLayer.Rules, "deep-water",
                AdjacentCondition(6, -1,
                    OrCondition((or) => {
                        or.Add(IfCondition(CheckTileType("current", "ocean")));
                        or.Add(IfCondition(CheckTileType("current", "water")));
                    })
                ),
                (list) => {
                    list.Add(OutcomeTileType("ocean", LiteralFloat(3)));
                });

            Layer<BasicTileInfo> firstLayer = recycler.GetLayer();
            scout.Layers.Add(firstLayer);
            firstLayer.Name = "types";
            firstLayer.Repeats = 2;
            AddRule(firstLayer.Rules, "free-tiles",
                IfCondition(CheckTileType("current", "void")),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(2)));
                    list.Add(OutcomeTileType("dirt", LiteralFloat(0.6f)));
                    list.Add(OutcomeTileType("desert", LiteralFloat(0.2f)));
                    list.Add(OutcomeTileType("swamp", LiteralFloat(0.2f)));
                    list.Add(OutcomeTileType("water", LiteralFloat(0.1f)));
                });

            AddRule(firstLayer.Rules, "stay-dirt",
                IfCondition(CheckTileType("current", "dirt")),
                (list) => {
                    list.Add(OutcomeTileType("dirt", LiteralFloat(1f)));
                });

            AddRule(firstLayer.Rules, "stay-grass",
                IfCondition(CheckTileType("current", "grass")),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(2f)));
                });

            AddRule(firstLayer.Rules, "stay-ocean",
                IfCondition(CheckTileType("current", "ocean")),
                (list) => {
                    list.Add(OutcomeTileType("ocean", LiteralFloat(20f)));
                });

            AddRule(firstLayer.Rules, "stay-water",
                IfCondition(CheckTileType("current", "water")),
                (list) => {
                    list.Add(OutcomeTileType("water", LiteralFloat(2f)));
                });

            AddRule(firstLayer.Rules, "stay-desert",
                IfCondition(CheckTileType("current", "desert")),
                (list) => {
                    list.Add(OutcomeTileType("desert", LiteralFloat(1f)));
                });

            AddRule(firstLayer.Rules, "stay-swamp",
                IfCondition(CheckTileType("current", "swamp")),
                (list) => {
                    list.Add(OutcomeTileType("swamp", LiteralFloat(1f)));
                });

            AddRule(firstLayer.Rules, "preserve-clump",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileType("current", "void", "!=")));
                    list.Add(AdjacentCondition(3, -1,
                        IfCondition(CompareTileType("current", "back-1"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileTypeByKeyword("current", "type", LiteralFloat(2.5f)));
                });

            AddRule(firstLayer.Rules, "evaporate",
                AndCondition((list) => {
                    list.Add(IfCondition(CheckTileArchetype("current", "water")));
                    list.Add(AdjacentCondition(0, 0,
                        IfCondition(CheckTileArchetype("current", "water"))
                    ));
                }),
                (list) => {
                    list.Add(OutcomeTileType("dirt", LiteralFloat(2.5f)));
                    list.Add(OutcomeTileType("grass", LiteralFloat(2.5f)));
                });

            AddRule(firstLayer.Rules, "more-grass",
                AdjacentCondition(1, -1,
                    IfCondition(CheckTileType("current", "grass"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(0.5f)));
                });

            AddRule(firstLayer.Rules, "clump-grass",
                AdjacentCondition(3, 5,
                    IfCondition(CheckTileType("current", "grass"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(1.5f)));
                });

            AddRule(firstLayer.Rules, "more-desert",
                AdjacentCondition(1, -1,
                    IfCondition(CheckTileType("current", "desert"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", LiteralFloat(1.5f)));
                    list.Add(OutcomeTileType("dirt", LiteralFloat(0.5f)));
                });

            AddRule(firstLayer.Rules, "even-more-desert",
                AdjacentCondition(3, -1,
                    IfCondition(CheckTileType("current", "desert"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", LiteralFloat(1.5f)));
                });

            AddRule(firstLayer.Rules, "far-desert",
                AdjacentCondition(1, -1,
                    AdjacentCondition(1, -1,
                        IfCondition(CheckTileType("current", "desert"))
                    )
                ),
                (list) => {
                    list.Add(OutcomeTileType("desert", LiteralFloat(0.2f)));
                });

            AddRule(firstLayer.Rules, "more-water",
                AdjacentCondition(2, -1, 0, 2,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", LiteralFloat(1.5f)));
                });

            AddRule(firstLayer.Rules, "clump-water",
                AdjacentCondition(6, -1, 0, 3,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", LiteralFloat(3.5f)));
                });

            AddRule(firstLayer.Rules, "flood-water",
                AdjacentCondition(5, -1,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("water", LiteralFloat(3.5f)));
                });

            AddRule(firstLayer.Rules, "start-river",
                AdjacentCondition(2, -1,
                    IfCondition(CheckTileType("current", "water"))
                ),
                (list) => {
                    list.Add(OutcomeTileType("river", LiteralFloat(0.25f)));
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
                    list.Add(OutcomeTileType("shallows", LiteralFloat(0.5f)));
                    list.Add(OutcomeTileType("beach", LiteralFloat(0.5f)));
                });

            AddRule(firstLayer.Rules, "swamp-growth",
                AdjacentCondition(1, -1,
                    AdjacentCondition(1, -1,
                        IfCondition(CheckTileType("current", "swamp"))
                    )
                ),
                (list) => {
                    list.Add(OutcomeTileType("swamp", LiteralFloat(0.5f)));
                    list.Add(OutcomeTileType("shallows", LiteralFloat(0.1f)));
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
                    list.Add(OutcomeTileType("swamp", LiteralFloat(1.5f)));
                    list.Add(OutcomeTileType("shallows", LiteralFloat(0.5f)));
                });

            AddRule(firstLayer.Rules, "swamp-harden",
                AdjacentCondition(5, -1,
                    OrCondition((list) => {
                        list.Add(IfCondition(CheckTileType("current", "swamp")));
                        list.Add(IfCondition(CheckTileType("current", "shallows")));
                    })
                ),
                (list) => {
                    list.Add(OutcomeTileType("grass", LiteralFloat(1.5f)));
                    list.Add(OutcomeTileType("dirt", LiteralFloat(1.5f)));
                });

            Layer<BasicTileInfo> riverLayer = recycler.GetLayer();
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
                    list.Add(OutcomeTileType("river", LiteralFloat(100f)));
                });

            Layer<BasicTileInfo> secondLayer = recycler.GetLayer();
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
                    list.Add(OutcomeTileHeight("flat", LiteralFloat(1f)));
                    list.Add(OutcomeTileHeight("hills", LiteralFloat(0.3f)));
                    list.Add(OutcomeTileHeight("mountains", LiteralFloat(0.1f)));
                });

            Layer<BasicTileInfo> thirdLayer = recycler.GetLayer();
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
                    list.Add(OutcomeTileFeature("empty", LiteralFloat(1f)));
                    list.Add(OutcomeTileFeature("forest", LiteralFloat(0.3f)));
                });
        }
    }
}
