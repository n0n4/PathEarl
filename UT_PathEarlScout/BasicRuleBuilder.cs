using PathEarlCore;
using PathEarlScout;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

using static PathEarlScout.RuleBuilder<UT_PathEarlScout.BasicTileInfo>;

namespace UT_PathEarlScout
{
    public static class BasicRuleBuilder
    {
        public static ScoutCheck<BasicTileInfo> CheckTileType(string owner, string type, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "type"), null, comparison, null, KeywordString(null, null, type), null);
        }

        public static ScoutCheck<BasicTileInfo> CompareTileType(string owner, string otherOwner, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "type"), null, comparison, null, KeywordString(otherOwner, "type"), null);
        }

        public static ScoutCheck<BasicTileInfo> CheckTileArchetype(string owner, string type, string comparison = "==")
        {
            return Check(null, KeywordString(owner, "archetype"), null, comparison, null, KeywordString(null, null, type), null);
        }

        public static Outcome<BasicTileInfo> OutcomeTileType(string type,
            KeywordFloat<BasicTileInfo> prob = null, string owner = "current")
        {
            return OutcomeString(LiteralString(owner), LiteralString("type"), "=", null, KeywordString(null, null, type), null, prob);
        }

        public static Outcome<BasicTileInfo> OutcomeTileTypeByKeyword(string owner,
            string keyword, KeywordFloat<BasicTileInfo> prob = null, string tileOwner = "current")
        {
            return OutcomeString(LiteralString(tileOwner), LiteralString("type"), "=", null, KeywordString(owner, keyword), null, prob);
        }

        public static Outcome<BasicTileInfo> OutcomeTileFeature(string feature,
            KeywordFloat<BasicTileInfo> prob = null, string owner = "current")
        {
            return OutcomeString(LiteralString(owner), LiteralString("feature"), "=", null, KeywordString(null, null, feature), null, prob);
        }

        public static Outcome<BasicTileInfo> OutcomeTileHeight(string height,
            KeywordFloat<BasicTileInfo> prob = null, string owner = "current")
        {
            return OutcomeString(LiteralString(owner), LiteralString("height"), "=", null, KeywordString(null, null, height), null, prob);
        }
    }
}
