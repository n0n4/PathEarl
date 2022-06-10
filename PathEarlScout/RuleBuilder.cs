using PathEarlCore;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public static class RuleBuilder<T> where T : ITileInfo
    {
        private static ScoutRecycler<T> Recycler;
        private static InfoAccess<T> InfoAccess;

        public static void Setup(ScoutRecycler<T> recycler, InfoAccess<T> infoAccess)
        {
            Recycler = recycler;
            InfoAccess = infoAccess;
        }

        public static Rule<T> AddRule(List<Rule<T>> rules,
            string name, ICondition<T> condition, Action<List<Outcome<T>>> addOutcomes)
        {
            Rule<T> rule = Recycler.GetRule();
            rules.Add(rule);
            rule.Name = name;
            rule.Condition = condition;
            addOutcomes(rule.Outcomes);
            return rule;
        }

        public static ConditionAnd<T> AndCondition(Action<List<ICondition<T>>> addConditions)
        {
            ConditionAnd<T> cond = Recycler.GetCondition(ConditionAnd<T>.Keyword) as ConditionAnd<T>;
            addConditions(cond.Conditions);
            return cond;
        }

        public static ConditionOr<T> OrCondition(Action<List<ICondition<T>>> addConditions)
        {
            ConditionOr<T> cond = Recycler.GetCondition(ConditionOr<T>.Keyword) as ConditionOr<T>;
            addConditions(cond.Conditions);
            return cond;
        }

        public static ConditionNot<T> NotCondition(Action<List<ICondition<T>>> addConditions)
        {
            ConditionNot<T> cond = Recycler.GetCondition(ConditionNot<T>.Keyword) as ConditionNot<T>;
            addConditions(cond.Conditions);
            return cond;
        }

        public static ConditionNot<T> NotCondition(ICondition<T> condition)
        {
            return NotCondition((not) => { not.Add(condition); });
        }

        public static ConditionAdjacent<T> AdjacentCondition(int min, int max, ICondition<T> condition)
        {
            ConditionAdjacent<T> cond = Recycler.GetCondition(ConditionAdjacent<T>.Keyword) as ConditionAdjacent<T>;
            cond.Min = min;
            cond.Max = max;
            cond.Condition = condition;
            return cond;
        }

        public static ConditionAdjacent<T> AdjacentCondition(int min, int max, int minRange, int maxRange, ICondition<T> condition)
        {
            ConditionAdjacent<T> cond = Recycler.GetCondition(ConditionAdjacent<T>.Keyword) as ConditionAdjacent<T>;
            cond.Min = min;
            cond.Max = max;
            cond.StartRange = minRange;
            cond.StopRange = maxRange;
            cond.Condition = condition;
            return cond;
        }

        public static ConditionAlways<T> AlwaysCondition()
        {
            return Recycler.GetCondition(ConditionAlways<T>.Keyword) as ConditionAlways<T>;
        }

        public static ConditionIf<T> IfCondition(ScoutCheck<T> check)
        {
            ConditionIf<T> cond = Recycler.GetCondition(ConditionIf<T>.Keyword) as ConditionIf<T>;
            cond.Check = check;
            return cond;
        }

        public static ScoutCheck<T> Check(
            KeywordFloat<T> firstFloat, KeywordString<T> firstString, KeywordInt<T> firstInt,
            string operation,
            KeywordFloat<T> secondFloat, KeywordString<T> secondString, KeywordInt<T> secondInt)
        {
            ScoutCheck<T> check = Recycler.ScoutCheckPool.Request();
            check.FirstKeywordFloat = firstFloat;
            check.FirstKeywordInt = firstInt;
            check.FirstKeywordString = firstString;
            check.Operation = operation;
            check.SecondKeywordFloat = secondFloat;
            check.SecondKeywordInt = secondInt;
            check.SecondKeywordString = secondString;

            return check;
        }

        public static ScoutCheck<T> CheckGlobalInt(string type, int value, string comparison = "==")
        {
            return Check(null, null, KeywordInt("ints", type), comparison, null, null, LiteralInt(value));
        }

        public static ScoutCheck<T> CheckGlobalFloat(string type, float value, string comparison = "==")
        {
            return Check(KeywordFloat("floats", type), null, null, comparison, LiteralFloat(value), null, null);
        }

        public static ScoutCheck<T> CheckGlobalString(string type, string value, string comparison = "==")
        {
            return Check(null, KeywordString("strings", type), null, comparison, null, LiteralString(value), null);
        }

        public static KeywordFloat<T> KeywordFloat(
            KeywordString<T> owner, KeywordString<T> keyword,
            float literal = 0, string nextop = null, KeywordFloat<T> next = null)
        {
            KeywordFloat<T> key = Recycler.KeywordFloatPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null && keyword.Literal != null && owner != null && owner.Literal != null)
                InfoAccess.TryGetFloatGet(key.KeywordOwner.Literal, key.Keyword.Literal, out key.Accessor);

            key.HasNext = next != null;
            key.NextFloat = next;
            key.NextOperation = nextop;
            return key;
        }

        public static KeywordFloat<T> KeywordFloat(string owner, string keyword)
        {
            return KeywordFloat(LiteralString(owner), LiteralString(keyword));
        }

        public static KeywordFloat<T> LiteralFloat(float f)
        {
            return KeywordFloat(null, null, f);
        }

        public static KeywordInt<T> KeywordInt(KeywordString<T> owner, KeywordString<T> keyword, int literal = 0, string nextop = null, KeywordInt<T> next = null)
        {
            KeywordInt<T> key = Recycler.KeywordIntPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null && keyword.Literal != null && owner != null && owner.Literal != null)
                InfoAccess.TryGetIntGet(key.KeywordOwner.Literal, key.Keyword.Literal, out key.Accessor);

            key.HasNext = next != null;
            key.NextInt = next;
            key.NextOperation = nextop;
            return key;
        }

        public static KeywordInt<T> KeywordInt(string owner, string keyword)
        {
            return KeywordInt(LiteralString(owner), LiteralString(keyword));
        }

        public static KeywordInt<T> LiteralInt(int i)
        {
            return KeywordInt(null, null, i);
        }

        public static KeywordString<T> KeywordString(KeywordString<T> owner, KeywordString<T> keyword, string literal = null, string nextop = null, KeywordString<T> next = null, KeywordInt<T> nextInt = null, KeywordFloat<T> nextFloat = null)
        {
            KeywordString<T> key = Recycler.KeywordStringPool.Request();
            key.KeywordOwner = owner;
            key.Keyword = keyword;
            key.Literal = literal;

            // get accessor
            if (keyword != null && keyword.Literal != null && keyword.HasNext == false 
                && owner != null && owner.Literal != null && owner.HasNext == false)
                InfoAccess.TryGetStringGet(key.KeywordOwner.Literal, key.Keyword.Literal, out key.Accessor);

            key.HasNext = nextop != null;
            key.NextString = next;
            key.NextInt = nextInt;
            key.NextFloat = nextFloat;
            key.NextOperation = nextop;
            return key;
        }

        public static KeywordString<T> KeywordString(string owner, string keyword)
        {
            return KeywordString(LiteralString(owner), LiteralString(keyword));
        }

        public static KeywordString<T> LiteralString(string value)
        {
            return KeywordString(null, null, value);
        }

        public static Outcome<T> OutcomeString(KeywordString<T> owner, KeywordString<T> keyword, string operation,
            KeywordFloat<T> keywordFloat, KeywordString<T> keywordString,
            KeywordInt<T> keywordInt, KeywordFloat<T> probability = null)
        {
            Outcome<T> outcome = Recycler.GetOutcome();
            outcome.KeywordString = KeywordString(owner, keyword);
            outcome.Operation = operation;
            outcome.ValueFloat = keywordFloat;
            outcome.ValueInt = keywordInt;
            outcome.ValueString = keywordString;
            outcome.Probability = probability;
            return outcome;
        }

        public static Outcome<T> OutcomeInt(KeywordString<T> owner, KeywordString<T> keyword, string operation,
            KeywordFloat<T> keywordFloat, KeywordString<T> keywordString,
            KeywordInt<T> keywordInt, KeywordFloat<T> probability = null)
        {
            Outcome<T> outcome = Recycler.GetOutcome();
            outcome.KeywordInt = KeywordInt(owner, keyword);
            outcome.Operation = operation;
            outcome.ValueFloat = keywordFloat;
            outcome.ValueInt = keywordInt;
            outcome.ValueString = keywordString;
            outcome.Probability = probability;
            return outcome;
        }

        public static Outcome<T> OutcomeFloat(KeywordString<T> owner, KeywordString<T> keyword, string operation,
            KeywordFloat<T> keywordFloat, KeywordString<T> keywordString,
            KeywordInt<T> keywordInt, KeywordFloat<T> probability = null)
        {
            Outcome<T> outcome = Recycler.GetOutcome();
            outcome.KeywordFloat = KeywordFloat(owner, keyword);
            outcome.Operation = operation;
            outcome.ValueFloat = keywordFloat;
            outcome.ValueInt = keywordInt;
            outcome.ValueString = keywordString;
            outcome.Probability = probability;
            return outcome;
        }

        public static Outcome<T> SimpleOutcomeString(string tileOwner, string tileType,
            string result,
            KeywordFloat<T> prob = null)
        {
            return OutcomeString(LiteralString(tileOwner), LiteralString(tileType), "=", null, KeywordString(null, null, result), null, prob);
        }

        public static Outcome<T> SimpleOutcomeKeywordString(string tileOwner, string tileType,
            string result,
            KeywordFloat<T> prob = null)
        {
            return OutcomeString(LiteralString(tileOwner), LiteralString(tileType), "=", null, LiteralString(result), null, prob);
        }

        public static Outcome<T> SimpleOutcomeKeywordString(string tileOwner, string tileType,
            string resultOwner, string resultType,
            KeywordFloat<T> prob = null)
        {
            return OutcomeString(LiteralString(tileOwner), LiteralString(tileType), "=", null, KeywordString(resultOwner, resultType), null, prob);
        }

        public static Outcome<T> SimpleOutcomeFloat(string tileOwner, string tileType,
            float result,
            KeywordFloat<T> prob = null)
        {
            return OutcomeFloat(LiteralString(tileOwner), LiteralString(tileType), "=", KeywordFloat(null, null, result), null, null, prob);
        }

        public static Outcome<T> SimpleOutcomeKeywordFloat(string tileOwner, string tileType,
            string resultOwner, string resultType,
            KeywordFloat<T> prob = null)
        {
            return OutcomeFloat(LiteralString(tileOwner), LiteralString(tileType), "=", KeywordFloat(resultOwner, resultType), null, null, prob);
        }

        public static Outcome<T> SimpleOutcomeInt(string tileOwner, string tileType,
            int result,
            KeywordFloat<T> prob = null)
        {
            return OutcomeInt(LiteralString(tileOwner), LiteralString(tileType), "=", null, null, KeywordInt(null, null, result), prob);
        }

        public static Outcome<T> SimpleOutcomeKeywordInt(string tileOwner, string tileType,
            string resultOwner, string resultType,
            KeywordFloat<T> prob = null)
        {
            return OutcomeInt(LiteralString(tileOwner), LiteralString(tileType), "=", null, null, KeywordInt(resultOwner, resultType), prob);
        }
    }
}
