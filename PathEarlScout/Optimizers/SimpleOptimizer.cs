using PathEarlCore;
using PathEarlScout.Keywords;
using PathEarlScout.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PathEarlScout.Optimizers
{
    public class SimpleOptimizer<T> : IOptimizer<T> where T : ITileInfo
    {
        public const string Keyword = "simple";

        public Random Random = new Random();
        private List<int> TileList = new List<int>();
        private List<int> NextTileList = new List<int>();
        private List<int> FailedTileList = new List<int>();
        private Dictionary<KeywordName, Dictionary<string, float>> StringOutcomes = new Dictionary<KeywordName, Dictionary<string, float>>();
        private List<Dictionary<string, float>> UnusedStringOutcomeDicts = new List<Dictionary<string, float>>();
        private Dictionary<KeywordName, Dictionary<float, float>> FloatOutcomes = new Dictionary<KeywordName, Dictionary<float, float>>();
        private List<Dictionary<float, float>> UnusedFloatOutcomeDicts = new List<Dictionary<float, float>>();
        private Dictionary<KeywordName, Dictionary<int, float>> IntOutcomes = new Dictionary<KeywordName, Dictionary<int, float>>();
        private List<Dictionary<int, float>> UnusedIntOutcomeDicts = new List<Dictionary<int, float>>();

        private List<KeywordName> ScratchNames = new List<KeywordName>();
        private List<string> ScratchStrings2 = new List<string>();
        private List<int> ScratchInts = new List<int>();
        private List<float> ScratchFloats = new List<float>();
        private MapScratch<T> MapScratch = new MapScratch<T>();
        private Dictionary<int, int> ScratchIntDict = new Dictionary<int, int>();
        private Dictionary<int, List<int>> ScratchIntListDict = new Dictionary<int, List<int>>();

        private List<Outcome<T>> ValidOutcomesScratch = new List<Outcome<T>>();

        public string GetKeyword()
        {
            return Keyword;
        }

        public void SetSeed(int seed)
        {
            Random = new Random(seed);
        }

        private void AddOutcomes(InfoAccess<T> access, Rule<T> rule, KeywordContext<T> context)
        {
            foreach (Outcome<T> outcome in rule.Outcomes)
            {
                string owner = outcome.GetOwner(context);
                string keyword = outcome.GetKeyword(context);
                if (access.TryGetFloatGet(owner, keyword, out Func<Tile<T>, float> getFloat))
                {
                    AddFloatOutcome(outcome, owner, keyword, getFloat, context);
                }
                else if (access.TryGetIntGet(owner, keyword, out Func<Tile<T>, int> getInt))
                {
                    AddIntOutcome(outcome, owner, keyword, getInt, context);
                }
                else if (access.TryGetStringGet(owner, keyword, out Func<Tile<T>, string> getString))
                {
                    AddStringOutcome(outcome, owner, keyword, getString, context);
                }
                else
                {
                    throw new Exception("Keyword '" + keyword + "' not recognized");
                }
            }
        }

        private void AddFloatOutcome(Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            float prob = outcome.GetProbability(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddFloatOutcomeValue(owner, keyword, outcome.Operation, value, prob, getFloat, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddFloatOutcomeValue(owner, keyword, outcome.Operation, value, prob, getFloat, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (float.TryParse(value, out float valueFloat))
                    AddFloatOutcomeValue(owner, keyword, outcome.Operation, valueFloat, prob, getFloat, context);
                else
                    throw new Exception("Could not parse '" + value + "' as float");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddFloatOutcomeValue(string owner, string keyword, string operation, float value, float prob, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            KeywordName name = new KeywordName(owner, keyword);
            if (!FloatOutcomes.TryGetValue(name, out Dictionary<float, float> dict))
            {
                if (UnusedFloatOutcomeDicts.Count > 0)
                {
                    dict = UnusedFloatOutcomeDicts[0];
                    UnusedFloatOutcomeDicts.RemoveAt(0);
                }
                else
                {
                    dict = new Dictionary<float, float>();
                }
                FloatOutcomes.Add(name, dict);
            }

            if (operation == "=")
            {
                if (!dict.ContainsKey(value))
                    dict.Add(value, prob);
                else
                    dict[value] += prob;
            } 
            else
            {
                float oldval = getFloat(context.GetTile(owner));
                float newval = KeywordHelper<T>.CombineFloats(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        private void AddIntOutcome(Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            float prob = outcome.GetProbability(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddIntOutcomeValue(owner, keyword, outcome.Operation, (int)value, prob, getInt, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddIntOutcomeValue(owner, keyword, outcome.Operation, value, prob, getInt, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (int.TryParse(value, out int valueInt))
                    AddIntOutcomeValue(owner, keyword, outcome.Operation, valueInt, prob, getInt, context);
                else
                    throw new Exception("Could not parse '" + value + "' as int");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddIntOutcomeValue(string owner, string keyword, string operation, int value, float prob, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            KeywordName name = new KeywordName(owner, keyword);
            if (!IntOutcomes.TryGetValue(name, out Dictionary<int, float> dict))
            {
                if (UnusedIntOutcomeDicts.Count > 0)
                {
                    dict = UnusedIntOutcomeDicts[0];
                    UnusedIntOutcomeDicts.RemoveAt(0);
                }
                else
                {
                    dict = new Dictionary<int, float>();
                }
                IntOutcomes.Add(name, dict);
            }

            if (operation == "=")
            {
                if (!dict.ContainsKey(value))
                    dict.Add(value, prob);
                else
                    dict[value] += prob;
            }
            else
            {
                int oldval = getInt(context.GetTile(owner));
                int newval = KeywordHelper<T>.CombineInts(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        private void AddStringOutcome(Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            float prob = outcome.GetProbability(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddStringOutcomeValue(owner, keyword, outcome.Operation, value.ToString(), prob, getString, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddStringOutcomeValue(owner, keyword, outcome.Operation, value.ToString(), prob, getString, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                AddStringOutcomeValue(owner, keyword, outcome.Operation, value, prob, getString, context);
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddStringOutcomeValue(string owner, string keyword, string operation, string value, float prob, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            KeywordName name = new KeywordName(owner, keyword);
            if (!StringOutcomes.TryGetValue(name, out Dictionary<string, float> dict))
            {
                if (UnusedStringOutcomeDicts.Count > 0)
                {
                    dict = UnusedStringOutcomeDicts[0];
                    UnusedStringOutcomeDicts.RemoveAt(0);
                }
                else
                {
                    dict = new Dictionary<string, float>();
                }
                StringOutcomes.Add(name, dict);
            }

            if (operation == "=")
            {
                if (!dict.ContainsKey(value))
                    dict.Add(value, prob);
                else
                    dict[value] += prob;
            }
            else
            {
                string oldval = getString(context.GetTile(owner));
                string newval = KeywordHelper<T>.CombineStrings(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        private void ExecuteOutcomes(InfoAccess<T> access, Tile<T> tile, List<Outcome<T>> outcomes, KeywordContext<T> context, float totalProb)
        {
            float cursum = 0;
            float r = (float)(Random.NextDouble() * totalProb);
            foreach (var outcome in outcomes)
            {
                cursum += outcome.GetProbability(context);
                if (r >= cursum)
                    continue;

                ExecuteOutcome(access, tile, outcome, context);
                break;
            }
        }

        private void ExecuteOutcome(InfoAccess<T> access, Tile<T> tile, Outcome<T> outcome, KeywordContext<T> context)
        {
            string owner = outcome.GetOwner(context);
            string keyword = outcome.GetKeyword(context);
            if (access.TryGetFloatGet(owner, keyword, out Func<Tile<T>, float> getFloat))
            {
                ExecuteFloatOutcome(tile, outcome, owner, keyword, getFloat, context);
            }
            else if (access.TryGetIntGet(owner, keyword, out Func<Tile<T>, int> getInt))
            {
                ExecuteIntOutcome(tile, outcome, owner, keyword, getInt, context);
            }
            else if (access.TryGetStringGet(owner, keyword, out Func<Tile<T>, string> getString))
            {
                ExecuteStringOutcome(tile, outcome, owner, keyword, getString, context);
            }
            else
            {
                throw new Exception("Keyword '" + keyword + "' not recognized");
            }
        }

        private void ExecuteFloatOutcome(Tile<T> tile, Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                ExecuteFloatOutcomeValue(tile, owner, keyword, outcome.Operation, value, getFloat, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                ExecuteFloatOutcomeValue(tile, owner, keyword, outcome.Operation, value, getFloat, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (float.TryParse(value, out float valueFloat))
                    ExecuteFloatOutcomeValue(tile, owner, keyword, outcome.Operation, valueFloat, getFloat, context);
                else
                    throw new Exception("Could not parse '" + value + "' as float");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void ExecuteFloatOutcomeValue(Tile<T> tile, string owner, string keyword, string operation, float value, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            if (!context.InfoAccess.TryGetFloatSet(owner, keyword, out Action<Tile<T>, float> func))
                throw new Exception("Float set method for '" + owner + "." + keyword + "' not found");
            
            if (operation == "=")
            {
                func(tile, value);
            }
            else
            {
                float oldval = getFloat(context.GetTile(owner));
                float newval = KeywordHelper<T>.CombineFloats(oldval, value, operation);
                func(tile, newval);
            }
        }

        private void ExecuteIntOutcome(Tile<T> tile, Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                ExecuteIntOutcomeValue(tile, owner, keyword, outcome.Operation, (int)value, getInt, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                ExecuteIntOutcomeValue(tile, owner, keyword, outcome.Operation, value, getInt, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (int.TryParse(value, out int valueInt))
                    ExecuteIntOutcomeValue(tile, owner, keyword, outcome.Operation, valueInt, getInt, context);
                else
                    throw new Exception("Could not parse '" + value + "' as int");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void ExecuteIntOutcomeValue(Tile<T> tile, string owner, string keyword, string operation, int value, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            if (!context.InfoAccess.TryGetIntSet(owner, keyword, out Action<Tile<T>, int> func))
                throw new Exception("Int set method for '" + owner + "." + keyword + "' not found");

            if (operation == "=")
            {
                func(tile, value);
            }
            else
            {
                int oldval = getInt(context.GetTile(owner));
                int newval = KeywordHelper<T>.CombineInts(oldval, value, operation);
                func(tile, newval);
            }
        }

        private void ExecuteStringOutcome(Tile<T> tile, Outcome<T> outcome, string owner, string keyword, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                ExecuteStringOutcomeValue(tile, owner, keyword, outcome.Operation, value.ToString(), getString, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                ExecuteStringOutcomeValue(tile, owner, keyword, outcome.Operation, value.ToString(), getString, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                ExecuteStringOutcomeValue(tile, owner, keyword, outcome.Operation, value, getString, context);
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void ExecuteStringOutcomeValue(Tile<T> tile, string owner, string keyword, string operation, string value, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            if (!context.InfoAccess.TryGetStringSet(owner, keyword, out Action<Tile<T>, string> func))
                throw new Exception("String set method for '" + owner + "." + keyword + "' not found");

            if (operation == "=")
            {
                func(tile, value);
            }
            else
            {
                string oldval = getString(context.GetTile(owner));
                string newval = KeywordHelper<T>.CombineStrings(oldval, value, operation);
                func(tile, newval);
            }
        }

        public void RunLayer(Scout<T> scout, Layer<T> layer, Layer<T> global)
        {
            // pull out some useful components
            Map<T> map = scout.Map;
            ScoutRecycler<T> recycler = scout.Recycler;
            KeywordContext<T> context = recycler.KeywordContextPool.Request();
            InfoAccess<T> infoAccess = scout.InfoAccess;

            // do global execution
            context.Clear();
            Tile<T> blankTile = context.Setup(map, infoAccess, map.Nodes.Keys.First());

            ValidOutcomesScratch.Clear();
            float totalProb = 0;
            foreach (Rule<T> rule in global.GlobalRules)
            {
                if (rule.Condition.Evaluate(context))
                {
                    foreach (Outcome<T> outcome in rule.Outcomes)
                    {
                        ValidOutcomesScratch.Add(outcome);
                        totalProb += outcome.GetProbability(context);
                    }
                }
            }
            ExecuteOutcomes(infoAccess, blankTile, ValidOutcomesScratch, context, totalProb);

            ValidOutcomesScratch.Clear();
            totalProb = 0;
            foreach (Rule<T> rule in layer.GlobalRules)
            {
                if (rule.Condition.Evaluate(context))
                {
                    foreach (Outcome<T> outcome in rule.Outcomes)
                    {
                        ValidOutcomesScratch.Add(outcome);
                        totalProb += outcome.GetProbability(context);
                    }
                }
            }
            ExecuteOutcomes(infoAccess, blankTile, ValidOutcomesScratch, context, totalProb);

            // put all tiles in random order
            TileList.Clear();
            FailedTileList.Clear();
            TileList.AddRange(map.Nodes.Keys);
            for (int i = 0; i < 7; i++)
                RandomHelper.Shuffle<int>(Random, TileList);

            // do autoexecution for all tiles
            for (int i = 0; i < TileList.Count; i++)
            {
                int id = TileList[i];
                context.Clear();
                Tile<T> tile = context.Setup(map, infoAccess, id);
                ValidOutcomesScratch.Clear();
                totalProb = 0;
                foreach (Rule<T> rule in global.AutoRules)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        foreach (Outcome<T> outcome in rule.Outcomes)
                        {
                            ValidOutcomesScratch.Add(outcome);
                            totalProb += outcome.GetProbability(context);
                        }
                    }
                }

                foreach (Rule<T> rule in layer.AutoRules)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        foreach (Outcome<T> outcome in rule.Outcomes)
                        {
                            ValidOutcomesScratch.Add(outcome);
                            totalProb += outcome.GetProbability(context);
                        }
                    }
                }
                ExecuteOutcomes(infoAccess, tile, ValidOutcomesScratch, context, totalProb);
            }

            // check every tile
            NextTileList.Clear();
            bool hitAny = false;
            bool firstFailPass = true;
            List<int> currentList = TileList;
            int runs = 0;
            for (int i = 0; i < currentList.Count; i++)
            {
                int id = currentList[i];

                // setup context
                context.Clear();
                Tile<T> tile = context.Setup(map, infoAccess, id);

                // clean outcomes dictionary
                foreach (var kvp in FloatOutcomes)
                {
                    kvp.Value.Clear();
                    UnusedFloatOutcomeDicts.Add(kvp.Value);
                }
                FloatOutcomes.Clear();

                foreach (var kvp in IntOutcomes)
                {
                    kvp.Value.Clear();
                    UnusedIntOutcomeDicts.Add(kvp.Value);
                }
                IntOutcomes.Clear();

                foreach (var kvp in StringOutcomes)
                {
                    kvp.Value.Clear();
                    UnusedStringOutcomeDicts.Add(kvp.Value);
                }
                StringOutcomes.Clear();

                // run all the rules and generate a list of possible outcomes
                foreach (Rule<T> rule in global.Rules)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        AddOutcomes(infoAccess, rule, context);
                    }
                    // tell the context to cleanup for the next rule
                    context.EndRule();
                }

                foreach (Rule<T> rule in layer.Rules)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        AddOutcomes(infoAccess, rule, context);
                    }
                    // tell the context to cleanup for the next rule
                    context.EndRule();
                }

                // pick one of the keywords that was shown
                // however, we can only pick one that has positive probabilities
                // so, clear out all options that have negative probabilities
                // and clear out all empty keywords after that
                CleanupOutcomes<float>(FloatOutcomes, UnusedFloatOutcomeDicts, ScratchFloats, ScratchNames);
                CleanupOutcomes<int>(IntOutcomes, UnusedIntOutcomeDicts, ScratchInts, ScratchNames);
                CleanupOutcomes<string>(StringOutcomes, UnusedStringOutcomeDicts, ScratchStrings2, ScratchNames);

                // now tally the keywords and pick one
                int keycount = FloatOutcomes.Keys.Count + StringOutcomes.Keys.Count + IntOutcomes.Keys.Count;
                if (keycount == 0)
                {
                    FailedTileList.Add(id);
                    continue; // no options!
                }
                hitAny = true;

                int randomkey = Random.Next(0, keycount);
                if (randomkey < FloatOutcomes.Count)
                {
                    float finalValue = RollOutcome<float>(FloatOutcomes, randomkey, out KeywordName finalKeyword);
                    if (!scout.InfoAccess.TryGetFloatSet(finalKeyword.Owner, finalKeyword.Keyword, out Action<Tile<T>, float> func))
                        throw new Exception("Float set method for '" + finalKeyword.Owner + "." + finalKeyword.Keyword + "' not found");
                    func(tile, finalValue);
                }
                else
                {
                    randomkey -= FloatOutcomes.Count;
                    if (randomkey < StringOutcomes.Count)
                    {
                        string finalValue = RollOutcome<string>(StringOutcomes, randomkey, out KeywordName finalKeyword);
                        if (!scout.InfoAccess.TryGetStringSet(finalKeyword.Owner, finalKeyword.Keyword, out Action<Tile<T>, string> func))
                            throw new Exception("String set method for '" + finalKeyword.Owner + "." + finalKeyword.Keyword + "' not found");
                        func(tile, finalValue);
                    }
                    else
                    {
                        randomkey -= StringOutcomes.Count;
                        int finalValue = RollOutcome<int>(IntOutcomes, randomkey, out KeywordName finalKeyword);
                        if (!scout.InfoAccess.TryGetIntSet(finalKeyword.Owner, finalKeyword.Keyword, out Action<Tile<T>, int> func))
                            throw new Exception("Int set method for '" + finalKeyword.Owner + "." + finalKeyword.Keyword + "' not found");
                        func(tile, finalValue);
                    }
                }

                // if this had multiple keywords in the outcome set, add it back to the list
                if (!layer.AutoCollapse && keycount > 1)
                {
                    NextTileList.Add(id);
                }

                // if we would be done, check if we have next tiles to deal with
                if (i + 1 >= currentList.Count)
                {
                    if (NextTileList.Count > 0)
                    {
                        currentList = NextTileList;
                        NextTileList = TileList;
                        TileList = currentList;
                        NextTileList.Clear();
                        i = 0;
                        runs++;
                    } 
                    else if (FailedTileList.Count > 0 && (hitAny || firstFailPass))
                    {
                        // (only continue trying failed tiles if we successfully updated
                        //  anything in the last pass-over, or if we haven't done a 
                        //  failed tiles pass-over at all yet)
                        currentList = FailedTileList;
                        FailedTileList = TileList;
                        TileList = currentList;
                        FailedTileList.Clear();
                        i = 0;
                        firstFailPass = false;
                        runs++;
                    }
                    hitAny = false;
                }
            }

            // run structures
            foreach (var structure in layer.Structures)
            {
                for (int i = 0; i < structure.Repeats + 1; i++)
                {
                    double r = Random.NextDouble();
                    if (r > structure.Rarity)
                        continue;

                    int pointCount = Random.Next(structure.MinPoints, structure.MaxPoints);
                    if (pointCount <= 0)
                        continue;

                    for (int p = 0; p < pointCount; p++)
                    {
                        // try to identify a valid point
                        int tstart = Random.Next(0, TileList.Count);
                        int tindex = tstart;
                        bool found = false;
                        Tile<T> tile = new Tile<T>();
                        for (int t = 0; t < TileList.Count; t++)
                        {
                            context.Clear();
                            tile = context.Setup(map, infoAccess, tindex);
                            if (structure.Condition == null || structure.Condition.Evaluate(context))
                            {
                                found = true;
                                break;
                            }

                            tindex++;
                            if (tindex >= TileList.Count)
                                tindex = 0;
                            if (tindex == tstart)
                                break;
                        }

                        if (!found)
                            break;

                        // execute at the point
                        foreach (var block in structure.Blocks)
                        {
                            int nextIndex = ExecuteStructureBlock(scout, block, tile, tindex);
                            if (nextIndex == -1)
                                break;
                            tindex = nextIndex;
                        }
                    }
                }
            }
        }

        private int ExecuteStructureBlock(Scout<T> scout, StructureBlock<T> block, Tile<T> tile, int tindex)
        {
            if (block.Cells.Count <= 0)
                return -1;

            // pull out some useful components
            Map<T> map = scout.Map;
            ScoutRecycler<T> recycler = scout.Recycler;
            KeywordContext<T> context = recycler.KeywordContextPool.Request();
            InfoAccess<T> infoAccess = scout.InfoAccess;

            if (block.Beam)
            {
                // try to identify a valid point
                int btstart = Random.Next(0, TileList.Count);
                int btindex = btstart;
                bool bfound = false;
                Tile<T> btile = new Tile<T>();
                for (int bt = 0; bt < TileList.Count; bt++)
                {
                    context.Clear();
                    btile = context.Setup(map, infoAccess, btindex);

                    bool failed = false;
                    if (block.BeamMaxDistance != 0 || block.BeamMinDistance != 0)
                    {
                        double dist = Math.Sqrt(Math.Pow(tile.X - btile.X, 2) + Math.Pow(tile.Y - btile.Y, 2));
                        if (dist < block.BeamMinDistance || (block.BeamMaxDistance != 0 && dist > block.BeamMinDistance))
                        {
                            failed = true;
                        }
                    }

                    if (!failed && (block.BeamTargetCondition == null || block.BeamTargetCondition.Evaluate(context)))
                    {
                        bfound = true;
                        break;
                    }

                    btindex++;
                    if (btindex >= TileList.Count)
                        btindex = 0;
                    if (btindex == btstart)
                        break;
                }

                if (!bfound)
                    return -1;

                // we have both points, so we can path between them now
                // use pathfinding to make a flow towards btile
                Dictionary<int, int> flow = null;
                if (block.BeamPathfinding == StructureBlock<T>.BEAM_PATHFINDING_EXPENSIVE)
                    flow = map.DjikstraFlowfield(btindex, EMapLayer.None, MapScratch);

                ScratchInts.Clear();
                ScratchInts.Add(tindex);

                int currentCellIndex = 0;

                int curi = tindex;
                while (curi != btindex)
                {
                    var conns = map.Nodes[curi];

                    // first, do we wander?
                    if (block.BeamWanderChance > 0)
                    {
                        for (int w = 0; w < block.BeamWanderRepeats + 1; w++)
                        {
                            if (Random.NextDouble() > block.BeamWanderChance)
                                continue;

                            // find a valid point
                            // - must meet beam condition
                            // - must not already be in ScratchInts

                            int nextIndex = -1;
                            for (int t = 0; t < conns.Count * 2; t++)
                            {
                                int index = Random.Next(0, conns.Count);
                                int cindex = conns.ElementAt(index).Key;
                                if (ScratchInts.Contains(cindex))
                                    continue;
                                context.Setup(map, infoAccess, cindex);

                                if (block.BeamWanderCondition != null && !block.BeamWanderCondition.Evaluate(context))
                                    continue;

                                nextIndex = cindex;
                                break;
                            }

                            if (nextIndex == -1)
                                break;

                            ExecuteStructureCell(scout, block.Cells[currentCellIndex], nextIndex);
                            currentCellIndex++;
                            if (currentCellIndex >= block.Cells.Count)
                                currentCellIndex = 0;

                            curi = nextIndex;
                            ScratchInts.Add(curi);
                            conns = map.Nodes[curi];
                        }
                    }

                    // next, move in correct direction
                    int correctIndex = -1;
                    if (flow != null)
                    {
                        if (!flow.ContainsKey(curi))
                        {
                            // the flow will not contain an entry for the target point
                            // so in this case we just stop
                            break;
                        }
                        correctIndex = flow[curi];
                    }
                    else
                    {
                        if (curi == btindex)
                            break;

                        correctIndex = map.FindClosestConnectionToPoint(curi, btile.X, btile.Y);
                        if (correctIndex == -1)
                            break;
                    }

                    ExecuteStructureCell(scout, block.Cells[currentCellIndex], correctIndex);
                    currentCellIndex++;
                    if (currentCellIndex >= block.Cells.Count)
                        currentCellIndex = 0;

                    curi = correctIndex;
                    ScratchInts.Add(curi);
                    if (!map.Nodes.ContainsKey(curi))
                    {
                        // might happen in rare cases
                        break;
                    }
                    conns = map.Nodes[curi];
                }

                // IMPORTANT: set tindex to our target index
                // so the next block picks up where this one left off
                return btindex;
            }

            // non-beam
            if (block.Cells.Count <= 0)
            {
                tindex++;
            }
            ExecuteStructureCell(scout, block.Cells[0], tindex);
            return tindex;
        }

        private void ExecuteStructureCell(Scout<T> scout, StructureCell<T> cell, int tindex)
        {
            ScratchIntDict.Clear();
            ScratchIntDict.Add(tindex, 0);
            ScratchIntListDict.Clear();

            float centerX = scout.Map.NodeX[tindex];
            float centerY = scout.Map.NodeY[tindex];

            ExecuteStructureCellOverConnections(scout, cell, tindex, 0, centerX, centerY);

            KeywordContext<T> context = scout.Recycler.KeywordContextPool.Request();
            int maxRadius = 0;
            foreach (var kvp in ScratchIntDict)
            {
                if (kvp.Value > maxRadius)
                    maxRadius = kvp.Value;

                if (!ScratchIntListDict.TryGetValue(kvp.Value, out List<int> list) || list == null)
                {
                    list = new List<int>();
                    ScratchIntListDict.Add(kvp.Value, list);
                }

                list.Add(kvp.Key);
            }

            // execute each rule over the whole set, one after the other
            foreach (var rule in cell.Rules)
            {
                for (int i = 0; i < maxRadius; i++)
                {
                    if (rule.MinRadius != null && i < rule.MinRadius)
                        continue;
                    if (rule.Radius != null && i > rule.Radius)
                        continue;

                    if (!ScratchIntListDict.TryGetValue(i, out List<int> list) || list == null)
                        continue;

                    foreach (int index in list)
                    {
                        ExecuteStructureCellRule(scout, cell, rule, context, index, i, centerX, centerY);
                    }
                }
            }
        }

        private void ExecuteStructureCellRule(Scout<T> scout, StructureCell<T> cell, StructureRule<T> cellrule, KeywordContext<T> context, int tileId, int currentRadius, float centerX, float centerY)
        {
            float x = scout.Map.NodeX[tileId] - centerX;
            float y = scout.Map.NodeY[tileId] - centerY;
            int minRadius = cellrule.MinRadius ?? cell.MinRadius;
            int radius = cellrule.Radius ?? cell.Radius;
            float minXOffset = cellrule.MinXOffset ?? cell.MinXOffset;
            float maxXOffset = cellrule.MaxXOffset ?? cell.MaxXOffset;
            float minYOffset = cellrule.MinYOffset ?? cell.MinYOffset;
            float maxYOffset = cellrule.MaxYOffset ?? cell.MaxYOffset;

            if (currentRadius + 1 >= minRadius && currentRadius + 1 <= radius
                && x >= minXOffset && x <= maxXOffset && y >= minYOffset && y <= maxYOffset)
            {
                context.Clear();
                var tile = context.Setup(scout.Map, scout.InfoAccess, tileId);
                if (cellrule.Rule.Condition == null || cellrule.Rule.Condition.Evaluate(context))
                {
                    float sum = 0;
                    ScratchFloats.Clear();
                    foreach (var outcome in cellrule.Rule.Outcomes)
                    {
                        var prob = outcome.GetProbability(context);
                        sum += prob;
                        ScratchFloats.Add(prob);
                    }

                    if (sum < 1)
                        sum = 1;

                    double r = Random.NextDouble() * sum;
                    float currentSum = 0;
                    for (int i = 0; i < cellrule.Rule.Outcomes.Count; i++)
                    {
                        currentSum += ScratchFloats[i];
                        if (r >= currentSum)
                            continue;

                        ExecuteOutcome(scout.InfoAccess, tile, cellrule.Rule.Outcomes[i], context);
                        break;
                    }
                }
            }
        }

        private void ExecuteStructureCellOverConnections(Scout<T> scout, StructureCell<T> cell, int tindex, int currentRadius, float centerX, float centerY)
        {
            var conns = scout.Map.Nodes[tindex];

            foreach (var conn in conns)
            {
                if (ScratchIntDict.TryGetValue(conn.Key, out int existing))
                {
                    if (existing > currentRadius + 1)
                        ScratchIntDict[conn.Key] = currentRadius + 1;
                    else
                        continue;
                }
                else
                {
                    ScratchIntDict.Add(conn.Key, currentRadius + 1);
                }

                if (currentRadius + 1 <= cell.Radius)
                    ExecuteStructureCellOverConnections(scout, cell, conn.Key, currentRadius + 1, centerX, centerY);
            }
        }

        private void CleanupOutcomes<U>(Dictionary<KeywordName, Dictionary<U, float>> outcomes,
            List<Dictionary<U, float>> unusedOutcomes, List<U> scratch, List<KeywordName> scratchNames)
        {
            scratch.Clear();
            scratchNames.Clear();
            foreach (var keyoutcomes in outcomes)
            {
                foreach (var values in keyoutcomes.Value)
                {
                    if (values.Value <= 0)
                    {
                        scratch.Add(values.Key);
                    }
                }

                foreach (U value in scratch)
                    keyoutcomes.Value.Remove(value);

                if (keyoutcomes.Value.Count <= 0)
                    scratchNames.Add(keyoutcomes.Key);
            }
            foreach (KeywordName k in scratchNames)
            {
                Dictionary<U, float> dict = outcomes[k];
                dict.Clear();
                unusedOutcomes.Add(dict);
                outcomes.Remove(k);
            }
        }

        private U RollOutcome<U>(Dictionary<KeywordName, Dictionary<U, float>> keyoutcomes, int randomkey, out KeywordName keyword)
        {
            keyword = new KeywordName();
            Dictionary<U, float> outcomes = null;
            foreach (var keyoutcome in keyoutcomes)
            {
                randomkey--;
                if (randomkey < 0)
                {
                    outcomes = keyoutcome.Value;
                    keyword = keyoutcome.Key;
                    break;
                }
            }
            if (outcomes == null)
                throw new Exception("Optimizer failed to assemble outcomes");

            double totalProb = 0;
            foreach (var option in outcomes)
                totalProb += option.Value;

            double rand = Random.NextDouble() * totalProb;
            foreach (var option in outcomes)
            {
                rand -= option.Value;
                if (rand < 0)
                {
                    return option.Key;
                }
            }
            // this shouldn't be possible, but as a failsafe:
            return outcomes.Keys.Last();
        }
    }
}
