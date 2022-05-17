using PathEarlCore;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using PathEarlScout.Optimizers;
using RelaStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class ScoutRecycler<T> where T : ITileInfo
    {
        public List<List<ICondition<T>>> UsedConditionLists = new List<List<ICondition<T>>>();
        public List<List<ICondition<T>>> UnusedConditionLists = new List<List<ICondition<T>>>();

        public List<ICondition<T>> UsedConditions = new List<ICondition<T>>();
        public Dictionary<string, List<ICondition<T>>> UnusedConditions = new Dictionary<string, List<ICondition<T>>>();

        public List<List<Outcome<T>>> UsedOutcomeLists = new List<List<Outcome<T>>>();
        public List<List<Outcome<T>>> UnusedOutcomeLists = new List<List<Outcome<T>>>();

        public List<Outcome<T>> UsedOutcomes = new List<Outcome<T>>();
        public List<Outcome<T>> UnusedOutcomes = new List<Outcome<T>>();

        public List<Layer<T>> UsedLayers = new List<Layer<T>>();
        public List<Layer<T>> UnusedLayers = new List<Layer<T>>();

        public List<List<Layer<T>>> UsedLayerLists = new List<List<Layer<T>>>();
        public List<List<Layer<T>>> UnusedLayerLists = new List<List<Layer<T>>>();

        public List<Rule<T>> UsedRules = new List<Rule<T>>();
        public List<Rule<T>> UnusedRules = new List<Rule<T>>();

        public List<List<Rule<T>>> UsedRuleLists = new List<List<Rule<T>>>();
        public List<List<Rule<T>>> UnusedRuleLists = new List<List<Rule<T>>>();

        public ListPool<KeywordContext<T>> KeywordContextPool;
        public ListPool<KeywordFloat<T>> KeywordFloatPool;
        public ListPool<KeywordInt<T>> KeywordIntPool;
        public ListPool<KeywordString<T>> KeywordStringPool;
        public ListPool<ParsedKeyword<T>> ParsedKeywordPool;
        public ListPool<ScoutCheck<T>> ScoutCheckPool;

        public Dictionary<string, IOptimizer<T>> Optimizers = new Dictionary<string, IOptimizer<T>>();

        public ScoutRecycler()
        {
            KeywordContextPool = new ListPool<KeywordContext<T>>(8, () => { return new KeywordContext<T>(); }, (k) => { k.Clear(); });
            KeywordFloatPool = new ListPool<KeywordFloat<T>>(8, () => { return new KeywordFloat<T>(); }, (k) => { k.Clear(); });
            KeywordIntPool = new ListPool<KeywordInt<T>>(8, () => { return new KeywordInt<T>(); }, (k) => { k.Clear(); });
            KeywordStringPool = new ListPool<KeywordString<T>>(8, () => { return new KeywordString<T>(); }, (k) => { k.Clear(); });
            ParsedKeywordPool = new ListPool<ParsedKeyword<T>>(8, () => { return new ParsedKeyword<T>(); }, (k) => { k.Clear(); });
            ScoutCheckPool = new ListPool<ScoutCheck<T>>(8, () => { return new ScoutCheck<T>(); }, (k) => { k.Clear(this); });
        }

        public ICondition<T> GetCondition(string keyword)
        {
            ICondition<T> result;
            if (UnusedConditions.TryGetValue(keyword, out List<ICondition<T>> conds)
                && conds.Count > 0)
            {
                result = conds[0];
                conds.RemoveAt(0);
                UsedConditions.Add(result);
                return result;
            }

            result = ConditionManager<T>.GetNew(keyword, this);
            UsedConditions.Add(result);
            return result;
        }
        
        public void ReturnCondition(ICondition<T> cond)
        {
            cond.Clear(this);
            if (!UnusedConditions.ContainsKey(cond.GetKeyword())) 
            {
                UnusedConditions.Add(cond.GetKeyword(), new List<ICondition<T>>());
            }

            UnusedConditions[cond.GetKeyword()].Add(cond);
            UsedConditions.Remove(cond);
        }

        public List<ICondition<T>> GetConditionList()
        {
            List<ICondition<T>> result;
            if (UnusedConditionLists.Count > 0)
            {
                result = UnusedConditionLists[0];
                UnusedConditionLists.RemoveAt(0);
                UsedConditionLists.Add(result);
                return result;
            }

            result = new List<ICondition<T>>();
            UsedConditionLists.Add(result);
            return result;
        }

        public void ReturnConditionList(List<ICondition<T>> list)
        {
            foreach (ICondition<T> cond in list)
            {
                ReturnCondition(cond);
            }
            list.Clear();

            UsedConditionLists.Remove(list);
            UnusedConditionLists.Add(list);
        }

        public Outcome<T> GetOutcome()
        {
            Outcome<T> result;
            if (UnusedOutcomes.Count > 0)
            {
                result = UnusedOutcomes[0];
                UnusedOutcomes.RemoveAt(0);
                UsedOutcomes.Add(result);
                return result;
            }

            result = new Outcome<T>(this);
            UsedOutcomes.Add(result);
            return result;
        }

        public void ReturnOutcome(Outcome<T> outcome)
        {
            outcome.Clear(this);
            UnusedOutcomes.Add(outcome);
            UsedOutcomes.Remove(outcome);
        }

        public List<Outcome<T>> GetOutcomeList()
        {
            List<Outcome<T>> result;
            if (UnusedOutcomeLists.Count > 0)
            {
                result = UnusedOutcomeLists[0];
                UnusedOutcomeLists.RemoveAt(0);
                UsedOutcomeLists.Add(result);
                return result;
            }

            result = new List<Outcome<T>>();
            UsedOutcomeLists.Add(result);
            return result;
        }

        public void ReturnOutcomeList(List<Outcome<T>> list)
        {
            foreach (Outcome<T> outcome in list)
            {
                ReturnOutcome(outcome);
            }
            list.Clear();

            UsedOutcomeLists.Remove(list);
            UnusedOutcomeLists.Add(list);
        }

        public Layer<T> GetLayer()
        {
            Layer<T> result;
            if (UnusedLayers.Count > 0)
            {
                result = UnusedLayers[0];
                UnusedLayers.RemoveAt(0);
                UsedLayers.Add(result);
                return result;
            }

            result = new Layer<T>(this);
            UsedLayers.Add(result);
            return result;
        }

        public void ReturnLayer(Layer<T> layer)
        {
            layer.Clear(this);
            UnusedLayers.Add(layer);
            UsedLayers.Remove(layer);
        }

        public List<Layer<T>> GetLayerList()
        {
            List<Layer<T>> result;
            if (UnusedLayerLists.Count > 0)
            {
                result = UnusedLayerLists[0];
                UnusedLayerLists.RemoveAt(0);
                UsedLayerLists.Add(result);
                return result;
            }

            result = new List<Layer<T>>();
            UsedLayerLists.Add(result);
            return result;
        }

        public void ReturnLayerList(List<Layer<T>> list)
        {
            foreach (Layer<T> layer in list)
            {
                ReturnLayer(layer);
            }
            list.Clear();

            UsedLayerLists.Remove(list);
            UnusedLayerLists.Add(list);
        }

        public Rule<T> GetRule()
        {
            Rule<T> result;
            if (UnusedRules.Count > 0)
            {
                result = UnusedRules[0];
                UnusedRules.RemoveAt(0);
                UsedRules.Add(result);
                return result;
            }

            result = new Rule<T>(this);
            UsedRules.Add(result);
            return result;
        }

        public void ReturnRule(Rule<T> rule)
        {
            rule.Clear(this);
            UnusedRules.Add(rule);
            UsedRules.Remove(rule);
        }

        public List<Rule<T>> GetRuleList()
        {
            List<Rule<T>> result;
            if (UnusedRuleLists.Count > 0)
            {
                result = UnusedRuleLists[0];
                UnusedRuleLists.RemoveAt(0);
                UsedRuleLists.Add(result);
                return result;
            }

            result = new List<Rule<T>>();
            UsedRuleLists.Add(result);
            return result;
        }

        public void ReturnRuleList(List<Rule<T>> list)
        {
            foreach (Rule<T> rule in list)
            {
                ReturnRule(rule);
            }
            list.Clear();

            UsedRuleLists.Remove(list);
            UnusedRuleLists.Add(list);
        }

        public IOptimizer<T> GetOptimizer(string keyword)
        {
            if (Optimizers.TryGetValue(keyword, out IOptimizer<T> opt))
            {
                return opt;
            }

            opt = OptimizerManager<T>.GetNew(keyword, this);
            Optimizers.Add(keyword, opt);
            return opt;
        }
    }
}
