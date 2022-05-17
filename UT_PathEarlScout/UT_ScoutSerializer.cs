using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathEarlCore;
using PathEarlScout;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UT_PathEarlScout
{
    [TestClass]
    public class UT_ScoutSerializer
    {
        public void AssertRulesListAreEqual(List<Rule<BasicTileInfo>> a, List<Rule<BasicTileInfo>> b)
        {
            Assert.AreEqual(a.Count, b.Count);

            for (int i = 0; i < a.Count; i++)
            {
                Rule<BasicTileInfo> ra = a[i];
                Rule<BasicTileInfo> rb = b[i];

                Assert.AreEqual(ra.Name, rb.Name);
                AssertConditionsAreEqual(ra.Condition, rb.Condition);
                Assert.AreEqual(ra.Outcomes.Count, rb.Outcomes.Count);
                for (int o = 0; o < ra.Outcomes.Count; o++)
                {
                    AssertOutcomesAreEqual(ra.Outcomes[o], rb.Outcomes[o]);
                }
            }
        }

        public void AssertOutcomesAreEqual(Outcome<BasicTileInfo> a, Outcome<BasicTileInfo> b)
        {
            AssertKeywordStringsAreEqual(a.Keyword, b.Keyword);
            Assert.AreEqual(a.Operation, b.Operation);
            AssertKeywordFloatsAreEqual(a.ValueFloat, b.ValueFloat);
            AssertKeywordIntsAreEqual(a.ValueInt, b.ValueInt);
            AssertKeywordStringsAreEqual(a.ValueString, b.ValueString);
        }

        public void AssertConditionsAreEqual(ICondition<BasicTileInfo> a, ICondition<BasicTileInfo> b)
        {
            Assert.AreEqual(a.GetKeyword(), b.GetKeyword());
            if (a is ConditionIf<BasicTileInfo>)
            {
                ConditionIf<BasicTileInfo> ca = a as ConditionIf<BasicTileInfo>;
                ConditionIf<BasicTileInfo> cb = b as ConditionIf<BasicTileInfo>;
                AssertChecksAreEqual(ca.Check, cb.Check);
            }
            else if (a is ConditionAdjacent<BasicTileInfo>)
            {
                ConditionAdjacent<BasicTileInfo> ca = a as ConditionAdjacent<BasicTileInfo>;
                ConditionAdjacent<BasicTileInfo> cb = b as ConditionAdjacent<BasicTileInfo>;
                Assert.AreEqual(ca.Min, cb.Min);
                Assert.AreEqual(ca.Max, cb.Max);
                AssertConditionsAreEqual(ca.Condition, cb.Condition);
            }
            else if (a is ConditionAnd<BasicTileInfo>)
            {
                ConditionAnd<BasicTileInfo> ca = a as ConditionAnd<BasicTileInfo>;
                ConditionAnd<BasicTileInfo> cb = b as ConditionAnd<BasicTileInfo>;
                Assert.AreEqual(ca.Conditions.Count, cb.Conditions.Count);
                for (int i = 0; i < ca.Conditions.Count; i++)
                {
                    AssertConditionsAreEqual(ca.Conditions[i], cb.Conditions[i]);
                }
            }
            else if (a is ConditionOr<BasicTileInfo>)
            {
                ConditionOr<BasicTileInfo> ca = a as ConditionOr<BasicTileInfo>;
                ConditionOr<BasicTileInfo> cb = b as ConditionOr<BasicTileInfo>;
                Assert.AreEqual(ca.Conditions.Count, cb.Conditions.Count);
                for (int i = 0; i < ca.Conditions.Count; i++)
                {
                    AssertConditionsAreEqual(ca.Conditions[i], cb.Conditions[i]);
                }
            }
            else if (a is ConditionNot<BasicTileInfo>)
            {
                ConditionNot<BasicTileInfo> ca = a as ConditionNot<BasicTileInfo>;
                ConditionNot<BasicTileInfo> cb = b as ConditionNot<BasicTileInfo>;
                Assert.AreEqual(ca.Conditions.Count, cb.Conditions.Count);
                for (int i = 0; i < ca.Conditions.Count; i++)
                {
                    AssertConditionsAreEqual(ca.Conditions[i], cb.Conditions[i]);
                }
            }
            else if (a is ConditionAtleast<BasicTileInfo>)
            {
                ConditionAtleast<BasicTileInfo> ca = a as ConditionAtleast<BasicTileInfo>;
                ConditionAtleast<BasicTileInfo> cb = b as ConditionAtleast<BasicTileInfo>;
                Assert.AreEqual(ca.Count, cb.Count);
                Assert.AreEqual(ca.Conditions.Count, cb.Conditions.Count);
                for (int i = 0; i < ca.Conditions.Count; i++)
                {
                    AssertConditionsAreEqual(ca.Conditions[i], cb.Conditions[i]);
                }
            }
        }

        public void AssertChecksAreEqual(ScoutCheck<BasicTileInfo> a, ScoutCheck<BasicTileInfo> b)
        {
            AssertKeywordFloatsAreEqual(a.FirstKeywordFloat, b.FirstKeywordFloat);
            AssertKeywordIntsAreEqual(a.FirstKeywordInt, b.FirstKeywordInt);
            AssertKeywordStringsAreEqual(a.FirstKeywordString, b.FirstKeywordString);
            Assert.AreEqual(a.Operation, b.Operation);
            AssertKeywordFloatsAreEqual(a.SecondKeywordFloat, b.SecondKeywordFloat);
            AssertKeywordIntsAreEqual(a.SecondKeywordInt, b.SecondKeywordInt);
            AssertKeywordStringsAreEqual(a.SecondKeywordString, b.SecondKeywordString);
        }

        public void AssertKeywordFloatsAreEqual(KeywordFloat<BasicTileInfo> a, KeywordFloat<BasicTileInfo> b)
        {
            if (a == null && b == null)
                return;
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.KeywordOwner, b.KeywordOwner);
            Assert.AreEqual(a.Keyword, b.Keyword);
            Assert.AreEqual(a.Literal, b.Literal);
            Assert.AreEqual(a.HasNext, b.HasNext);
            if (a.HasNext)
            {
                Assert.AreEqual(a.NextOperation, b.NextOperation);
                AssertKeywordFloatsAreEqual(a.NextFloat, b.NextFloat);
                AssertKeywordIntsAreEqual(a.NextInt, b.NextInt);
                AssertKeywordStringsAreEqual(a.NextString, b.NextString);
            }
        }

        public void AssertKeywordIntsAreEqual(KeywordInt<BasicTileInfo> a, KeywordInt<BasicTileInfo> b)
        {
            if (a == null && b == null)
                return;
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.KeywordOwner, b.KeywordOwner);
            Assert.AreEqual(a.Keyword, b.Keyword);
            Assert.AreEqual(a.Literal, b.Literal);
            Assert.AreEqual(a.HasNext, b.HasNext);
            if (a.HasNext)
            {
                Assert.AreEqual(a.NextOperation, b.NextOperation);
                AssertKeywordFloatsAreEqual(a.NextFloat, b.NextFloat);
                AssertKeywordIntsAreEqual(a.NextInt, b.NextInt);
                AssertKeywordStringsAreEqual(a.NextString, b.NextString);
            }
        }

        public void AssertKeywordStringsAreEqual(KeywordString<BasicTileInfo> a, KeywordString<BasicTileInfo> b)
        {
            if (a == null && b == null)
                return;
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.KeywordOwner, b.KeywordOwner);
            Assert.AreEqual(a.Keyword, b.Keyword);
            Assert.AreEqual(a.Literal, b.Literal);
            Assert.AreEqual(a.HasNext, b.HasNext);
            if (a.HasNext)
            {
                Assert.AreEqual(a.NextOperation, b.NextOperation);
                AssertKeywordFloatsAreEqual(a.NextFloat, b.NextFloat);
                AssertKeywordIntsAreEqual(a.NextInt, b.NextInt);
                AssertKeywordStringsAreEqual(a.NextString, b.NextString);
            }
        }

        [TestMethod]
        public void SaveLoadTest()
        {
            Map<BasicTileInfo> map = BasicScout.MakeHexMap(50, 50, out BasicTileInfo[,] infos);
            Scout<BasicTileInfo> scout = BasicScout.MakeScout(map, "basic-scout");
            BasicScout.AddBasicScoutRules(scout);

            ScoutSerializer<BasicTileInfo> serializer = new ScoutSerializer<BasicTileInfo>();

            using (var fileStream = new FileStream("SaveLoadText.txt", FileMode.Create)) 
            {
                using (var streamWriter = new StreamWriter(fileStream)) 
                {
                    serializer.Save(scout, streamWriter);
                }
            }

            // now try loading
            Scout<BasicTileInfo> loadScout = BasicScout.MakeScout(map, "load-scout");

            using (var fileStream = new FileStream("SaveLoadText.txt", FileMode.Open))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    serializer.Load(loadScout, streamReader);
                }
            }

            // are they equal?
            Assert.AreEqual(scout.Name, loadScout.Name);
            Assert.AreEqual(scout.Optimizer.GetKeyword(), loadScout.Optimizer.GetKeyword());

            AssertRulesListAreEqual(scout.GlobalRules, loadScout.GlobalRules);

            Assert.AreEqual(scout.Layers.Count, loadScout.Layers.Count);
            for (int i = 0; i < scout.Layers.Count; i++)
            {
                Layer<BasicTileInfo> la = scout.Layers[i];
                Layer<BasicTileInfo> lb = loadScout.Layers[i];
                Assert.AreEqual(la.Name, lb.Name);
                AssertRulesListAreEqual(la.Rules, lb.Rules);
            }
        }
    }
}
