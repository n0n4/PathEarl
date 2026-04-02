using PathEarlCore;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using PathEarlScout.Optimizers;
using PathEarlScout.Structures;
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

        public List<Structure<T>> UsedStructures = new List<Structure<T>>();
        public List<Structure<T>> UnusedStructures = new List<Structure<T>>();

        public List<List<Structure<T>>> UsedStructureLists = new List<List<Structure<T>>>();
        public List<List<Structure<T>>> UnusedStructureLists = new List<List<Structure<T>>>();

        public List<StructureBlock<T>> UsedStructureBlocks = new List<StructureBlock<T>>();
        public List<StructureBlock<T>> UnusedStructureBlocks = new List<StructureBlock<T>>();

        public List<List<StructureBlock<T>>> UsedStructureBlockLists = new List<List<StructureBlock<T>>>();
        public List<List<StructureBlock<T>>> UnusedStructureBlockLists = new List<List<StructureBlock<T>>>();

        public List<StructureCell<T>> UsedStructureCells = new List<StructureCell<T>>();
        public List<StructureCell<T>> UnusedStructureCells = new List<StructureCell<T>>();

        public List<List<StructureCell<T>>> UsedStructureCellLists = new List<List<StructureCell<T>>>();
        public List<List<StructureCell<T>>> UnusedStructureCellLists = new List<List<StructureCell<T>>>();

        public List<StructureRule<T>> UsedStructureRules = new List<StructureRule<T>>();
        public List<StructureRule<T>> UnusedStructureRules = new List<StructureRule<T>>();

        public List<List<StructureRule<T>>> UsedStructureRuleLists = new List<List<StructureRule<T>>>();
        public List<List<StructureRule<T>>> UnusedStructureRuleLists = new List<List<StructureRule<T>>>();

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

        public Structure<T> GetStructure()
        {
            Structure<T> result;
            if (UnusedStructures.Count > 0)
            {
                result = UnusedStructures[0];
                UnusedStructures.RemoveAt(0);
                UsedStructures.Add(result);
                return result;
            }

            result = new Structure<T>(this);
            UsedStructures.Add(result);
            return result;
        }

        public void ReturnStructure(Structure<T> structure)
        {
            structure.Clear(this);
            UnusedStructures.Add(structure);
            UsedStructures.Remove(structure);
        }

        public List<Structure<T>> GetStructureList()
        {
            List<Structure<T>> result;
            if (UnusedStructureLists.Count > 0)
            {
                result = UnusedStructureLists[0];
                UnusedStructureLists.RemoveAt(0);
                UsedStructureLists.Add(result);
                return result;
            }

            result = new List<Structure<T>>();
            UsedStructureLists.Add(result);
            return result;
        }

        public void ReturnStructureList(List<Structure<T>> list)
        {
            foreach (Structure<T> rule in list)
            {
                ReturnStructure(rule);
            }
            list.Clear();

            UsedStructureLists.Remove(list);
            UnusedStructureLists.Add(list);
        }


        public StructureBlock<T> GetStructureBlock()
        {
            StructureBlock<T> result;
            if (UnusedStructureBlocks.Count > 0)
            {
                result = UnusedStructureBlocks[0];
                UnusedStructureBlocks.RemoveAt(0);
                UsedStructureBlocks.Add(result);
                return result;
            }

            result = new StructureBlock<T>(this);
            UsedStructureBlocks.Add(result);
            return result;
        }

        public void ReturnStructureBlock(StructureBlock<T> structure)
        {
            structure.Clear(this);
            UnusedStructureBlocks.Add(structure);
            UsedStructureBlocks.Remove(structure);
        }

        public List<StructureBlock<T>> GetStructureBlockList()
        {
            List<StructureBlock<T>> result;
            if (UnusedStructureBlockLists.Count > 0)
            {
                result = UnusedStructureBlockLists[0];
                UnusedStructureBlockLists.RemoveAt(0);
                UsedStructureBlockLists.Add(result);
                return result;
            }

            result = new List<StructureBlock<T>>();
            UsedStructureBlockLists.Add(result);
            return result;
        }

        public void ReturnStructureBlockList(List<StructureBlock<T>> list)
        {
            foreach (StructureBlock<T> rule in list)
            {
                ReturnStructureBlock(rule);
            }
            list.Clear();

            UsedStructureBlockLists.Remove(list);
            UnusedStructureBlockLists.Add(list);
        }

        public StructureCell<T> GetStructureCell()
        {
            StructureCell<T> result;
            if (UnusedStructureCells.Count > 0)
            {
                result = UnusedStructureCells[0];
                UnusedStructureCells.RemoveAt(0);
                UsedStructureCells.Add(result);
                return result;
            }

            result = new StructureCell<T>(this);
            UsedStructureCells.Add(result);
            return result;
        }

        public void ReturnStructureCell(StructureCell<T> structure)
        {
            structure.Clear(this);
            UnusedStructureCells.Add(structure);
            UsedStructureCells.Remove(structure);
        }

        public List<StructureCell<T>> GetStructureCellList()
        {
            List<StructureCell<T>> result;
            if (UnusedStructureCellLists.Count > 0)
            {
                result = UnusedStructureCellLists[0];
                UnusedStructureCellLists.RemoveAt(0);
                UsedStructureCellLists.Add(result);
                return result;
            }

            result = new List<StructureCell<T>>();
            UsedStructureCellLists.Add(result);
            return result;
        }

        public void ReturnStructureCellList(List<StructureCell<T>> list)
        {
            foreach (StructureCell<T> rule in list)
            {
                ReturnStructureCell(rule);
            }
            list.Clear();

            UsedStructureCellLists.Remove(list);
            UnusedStructureCellLists.Add(list);
        }

        public List<StructureRule<T>> GetStructureRuleList()
        {
            List<StructureRule<T>> result;
            if (UnusedStructureRuleLists.Count > 0)
            {
                result = UnusedStructureRuleLists[0];
                UnusedStructureRuleLists.RemoveAt(0);
                UsedStructureRuleLists.Add(result);
                return result;
            }

            result = new List<StructureRule<T>>();
            UsedStructureRuleLists.Add(result);
            return result;
        }

        public void ReturnStructureRuleList(List<StructureRule<T>> list)
        {
            foreach (StructureRule<T> rule in list)
            {
                ReturnStructureRule(rule);
            }
            list.Clear();

            UsedStructureRuleLists.Remove(list);
            UnusedStructureRuleLists.Add(list);
        }

        public StructureRule<T> GetStructureRule()
        {
            StructureRule<T> result;
            if (UnusedStructureRules.Count > 0)
            {
                result = UnusedStructureRules[0];
                UnusedStructureRules.RemoveAt(0);
                UsedStructureRules.Add(result);
                return result;
            }

            result = new StructureRule<T>(this);
            UsedStructureRules.Add(result);
            return result;
        }

        public void ReturnStructureRule(StructureRule<T> structure)
        {
            structure.Clear(this);
            UnusedStructureRules.Add(structure);
            UsedStructureRules.Remove(structure);
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
