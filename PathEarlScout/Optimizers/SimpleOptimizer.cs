using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, Dictionary<string, float>> StringOutcomes = new Dictionary<string, Dictionary<string, float>>();
        private List<Dictionary<string, float>> UnusedStringOutcomeDicts = new List<Dictionary<string, float>>();
        private Dictionary<string, Dictionary<float, float>> FloatOutcomes = new Dictionary<string, Dictionary<float, float>>();
        private List<Dictionary<float, float>> UnusedFloatOutcomeDicts = new List<Dictionary<float, float>>();
        private Dictionary<string, Dictionary<int, float>> IntOutcomes = new Dictionary<string, Dictionary<int, float>>();
        private List<Dictionary<int, float>> UnusedIntOutcomeDicts = new List<Dictionary<int, float>>();

        private List<string> ScratchStrings = new List<string>();
        private List<string> ScratchStrings2 = new List<string>();
        private List<int> ScratchInts = new List<int>();
        private List<float> ScratchFloats = new List<float>();

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
                string keyword = outcome.Keyword.Value(context);
                if (access.GetFloats.TryGetValue(keyword, out Func<Tile<T>, float> getFloat))
                {
                    AddFloatOutcome(outcome, keyword, getFloat, context);
                }
                else if (access.GetInts.TryGetValue(keyword, out Func<Tile<T>, int> getInt))
                {
                    AddIntOutcome(outcome, keyword, getInt, context);
                }
                else if (access.GetStrings.TryGetValue(keyword, out Func<Tile<T>, string> getString))
                {
                    AddStringOutcome(outcome, keyword, getString, context);
                }
                else
                {
                    throw new Exception("Keyword '" + keyword + "' not recognized");
                }
            }
        }

        private void AddFloatOutcome(Outcome<T> outcome, string keyword, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            float prob = outcome.Probability.Value(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddFloatOutcomeValue(keyword, outcome.Operation, value, prob, getFloat, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddFloatOutcomeValue(keyword, outcome.Operation, value, prob, getFloat, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (float.TryParse(value, out float valueFloat))
                    AddFloatOutcomeValue(keyword, outcome.Operation, valueFloat, prob, getFloat, context);
                else
                    throw new Exception("Could not parse '" + value + "' as float");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddFloatOutcomeValue(string keyword, string operation, float value, float prob, Func<Tile<T>, float> getFloat, KeywordContext<T> context)
        {
            if (!FloatOutcomes.TryGetValue(keyword, out Dictionary<float, float> dict))
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
                FloatOutcomes.Add(keyword, dict);
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
                float oldval = getFloat(context.Tiles[0]);
                float newval = KeywordHelper<T>.CombineFloats(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        private void AddIntOutcome(Outcome<T> outcome, string keyword, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            float prob = outcome.Probability.Value(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddIntOutcomeValue(keyword, outcome.Operation, (int)value, prob, getInt, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddIntOutcomeValue(keyword, outcome.Operation, value, prob, getInt, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                if (int.TryParse(value, out int valueInt))
                    AddIntOutcomeValue(keyword, outcome.Operation, valueInt, prob, getInt, context);
                else
                    throw new Exception("Could not parse '" + value + "' as int");
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddIntOutcomeValue(string keyword, string operation, int value, float prob, Func<Tile<T>, int> getInt, KeywordContext<T> context)
        {
            if (!IntOutcomes.TryGetValue(keyword, out Dictionary<int, float> dict))
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
                IntOutcomes.Add(keyword, dict);
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
                int oldval = getInt(context.Tiles[0]);
                int newval = KeywordHelper<T>.CombineInts(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        private void AddStringOutcome(Outcome<T> outcome, string keyword, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            float prob = outcome.Probability.Value(context);
            if (outcome.ValueFloat != null)
            {
                float value = outcome.ValueFloat.Value(context);
                AddStringOutcomeValue(keyword, outcome.Operation, value.ToString(), prob, getString, context);
            }
            else if (outcome.ValueInt != null)
            {
                int value = outcome.ValueInt.Value(context);
                AddStringOutcomeValue(keyword, outcome.Operation, value.ToString(), prob, getString, context);
            }
            else if (outcome.ValueString != null)
            {
                string value = outcome.ValueString.Value(context);
                AddStringOutcomeValue(keyword, outcome.Operation, value, prob, getString, context);
            }
            else
            {
                throw new Exception("Expected outcome to have value.");
            }
        }

        private void AddStringOutcomeValue(string keyword, string operation, string value, float prob, Func<Tile<T>, string> getString, KeywordContext<T> context)
        {
            if (!StringOutcomes.TryGetValue(keyword, out Dictionary<string, float> dict))
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
                StringOutcomes.Add(keyword, dict);
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
                string oldval = getString(context.Tiles[0]);
                string newval = KeywordHelper<T>.CombineStrings(oldval, value, operation);
                if (!dict.ContainsKey(newval))
                    dict.Add(newval, prob);
                else
                    dict[newval] += prob;
            }
        }

        public void RunLayer(Scout<T> scout, Layer<T> layer, List<Rule<T>> globals)
        {
            // pull out some useful components
            Map<T> map = scout.Map;
            ScoutRecycler<T> recycler = scout.Recycler;
            KeywordContext<T> context = recycler.KeywordContextPool.Request();

            // put all tiles in random order
            TileList.Clear();
            TileList.AddRange(map.Nodes.Keys);
            for (int i = 0; i < 7; i++)
                RandomHelper.Shuffle<int>(Random, TileList);

            // check every tile
            NextTileList.Clear();
            bool hitAny = false;
            bool firstFailPass = true;
            List<int> currentList = TileList;
            for (int i = 0; i < currentList.Count; i++)
            {
                int id = currentList[i];

                // setup context
                context.Clear();
                Tile<T> tile = context.Setup(map, id);

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
                foreach (Rule<T> rule in globals)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        AddOutcomes(scout.InfoAccess, rule, context);
                    }
                    // tell the context to cleanup for the next rule
                    context.EndRule();
                }

                foreach (Rule<T> rule in layer.Rules)
                {
                    if (rule.Condition.Evaluate(context))
                    {
                        AddOutcomes(scout.InfoAccess, rule, context);
                    }
                    // tell the context to cleanup for the next rule
                    context.EndRule();
                }

                // pick one of the keywords that was shown
                // however, we can only pick one that has positive probabilities
                // so, clear out all options that have negative probabilities
                // and clear out all empty keywords after that
                CleanupOutcomes<float>(FloatOutcomes, UnusedFloatOutcomeDicts, ScratchFloats, ScratchStrings);
                CleanupOutcomes<int>(IntOutcomes, UnusedIntOutcomeDicts, ScratchInts, ScratchStrings);
                CleanupOutcomes<string>(StringOutcomes, UnusedStringOutcomeDicts, ScratchStrings2, ScratchStrings);

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
                    float finalValue = RollOutcome<float>(FloatOutcomes, randomkey, out string finalKeyword);
                    scout.InfoAccess.SetFloats[finalKeyword](tile, finalValue);
                }
                else
                {
                    randomkey -= FloatOutcomes.Count;
                    if (randomkey < StringOutcomes.Count)
                    {
                        string finalValue = RollOutcome<string>(StringOutcomes, randomkey, out string finalKeyword);
                        scout.InfoAccess.SetStrings[finalKeyword](tile, finalValue);
                    }
                    else
                    {
                        randomkey -= StringOutcomes.Count;
                        int finalValue = RollOutcome<int>(IntOutcomes, randomkey, out string finalKeyword);
                        scout.InfoAccess.SetInts[finalKeyword](tile, finalValue);
                    }
                }

                // if this had multiple keywords in the outcome set, add it back to the list
                if (keycount > 1)
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
                    }
                    hitAny = false;
                }
            }
        }

        private void CleanupOutcomes<U>(Dictionary<string, Dictionary<U, float>> outcomes,
            List<Dictionary<U, float>> unusedOutcomes, List<U> scratch, List<string> scratchStrings)
        {
            scratch.Clear();
            scratchStrings.Clear();
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
                    scratchStrings.Add(keyoutcomes.Key);
            }
            foreach (string k in scratchStrings)
            {
                Dictionary<U, float> dict = outcomes[k];
                dict.Clear();
                unusedOutcomes.Add(dict);
                outcomes.Remove(k);
            }
        }

        private U RollOutcome<U>(Dictionary<string, Dictionary<U, float>> keyoutcomes, int randomkey, out string keyword)
        {
            keyword = null;
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
