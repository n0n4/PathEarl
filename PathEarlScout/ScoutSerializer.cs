using PathEarlCore;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using PathEarlScout.Structures;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace PathEarlScout
{
    public class ScoutSerializer<T> where T : ITileInfo
    {
        private int TabLevel = 0;
        private string[] Tabs;

        public Scout<T> Scout;
        private StreamWriter Writer;
        private bool StartedLine = false;

        private StreamReader Reader;
        private string OriginalLastLine = null;
        public string LastLine = null;
        public bool UsedLastLine = true;
        public int LineCount = 0;

        public ScoutSerializer()
        {
            Tabs = new string[64];
            Tabs[0] = "";
            Tabs[1] = "\t";
            for (int i = 2; i < Tabs.Length; i++)
                Tabs[i] = Tabs[i - 1] + Tabs[1];
        }

        public void Load(Scout<T> scout, StreamReader reader)
        {
            Scout = scout;
            Reader = reader;
            LineCount = 0;
            TabLevel = 0;

            // expect first line to be name
            if (!TryReadLine() || !LastLine.ToLower().StartsWith("name ") || LastLine.Length < 6)
                throw new Exception("Expected Name at line " + LineCount);

            scout.Name = LastLine.Substring(5);

            // expect second line to be optimizer
            if (!TryReadLine() || !LastLine.ToLower().StartsWith("optimizer ") || LastLine.Length < 11)
                throw new Exception("Expected Optimizer at line " + LineCount);

            scout.Optimizer = scout.Recycler.GetOptimizer(LastLine.Substring(10));
            scout.Repeats = 0;

            // check for global or layers
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("repeats")) 
                {
                    string repeatText = LastLine.Substring(8);
                    if (!int.TryParse(repeatText, out scout.Repeats))
                        throw new Exception("Failed to convert '" + repeatText + "' to int at line " + LineCount);
                }
                else if (lowered.StartsWith("global"))
                {
                    LoadLayer(scout.GlobalLayer, "GLOBAL");
                } 
                else if (lowered.StartsWith("layers"))
                {
                    LoadLayerList(scout.Layers);
                }
            }
        }

        public void Save(Scout<T> scout, StreamWriter writer)
        {
            Scout = scout;
            Writer = writer;
            TabLevel = 0;

            Write("NAME ");
            WriteLine(scout.Name);

            Write("OPTIMIZER ");
            WriteLine(scout.Optimizer.GetKeyword());

            if (scout.Repeats != 0)
            {
                Write("REPEATS ");
                WriteLine(scout.Repeats.ToString());
            }

            WriteLine("GLOBAL");
            SaveLayer(scout.GlobalLayer, "GLOBAL");

            WriteLine("LAYERS");
            SaveLayerList(scout.Layers);
        }

        public void Write(string text)
        {
            if (text == null)
                text = string.Empty;

            if (!StartedLine)
            {
                Writer.Write(Tabs[TabLevel]);
                StartedLine = true;
            }
            Writer.Write(text);
        }

        public void WriteLine(string text)
        {
            if (text == null)
                text = string.Empty;

            if (!StartedLine)
            {
                Writer.Write(Tabs[TabLevel]);
                StartedLine = true;
            }
            Writer.WriteLine(text);
            StartedLine = false;
        }

        public bool TryReadLine()
        {
            if (Reader.EndOfStream)
                return false;

            if (UsedLastLine)
            {
                LastLine = Reader.ReadLine();
                OriginalLastLine = LastLine;
                LineCount++;
            }

            // return true if the lastline has the appropriate tabbing
            if (OriginalLastLine.StartsWith(Tabs[TabLevel]))
            {
                LastLine = OriginalLastLine.Substring(Tabs[TabLevel].Length);
                UsedLastLine = true;
                return true;
            }
            UsedLastLine = false;
            return false;
        }

        public void SaveLayerList(List<Layer<T>> list)
        {
            foreach (Layer<T> layer in list)
                SaveLayer(layer);
        }

        public void LoadLayerList(List<Layer<T>> list)
        {
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                Layer<T> layer = Scout.Recycler.GetLayer();
                LoadLayer(layer);
                list.Add(layer);
            }
        }

        public void SaveLayer(Layer<T> layer, string givenName = null)
        {
            if (givenName == null)
            {
                Write("LAYER ");
                WriteLine(layer.Name);
            }

            TabLevel++;
            if (layer.Repeats != 0)
            {
                Write("REPEATS ");
                WriteLine(layer.Repeats.ToString());
            }
            if (layer.AutoCollapse)
            {
                WriteLine("AUTOCOLLAPSE");
            }
            if (layer.GlobalRules.Count > 0)
            {
                WriteLine("GLOBAL");
                SaveRuleList(layer.GlobalRules);
            }
            if (layer.AutoRules.Count > 0)
            {
                WriteLine("AUTO");
                SaveRuleList(layer.AutoRules);
            }
            if (layer.Structures.Count > 0)
            {
                WriteLine("STRUCTS");
                SaveStructureList(layer.Structures);
            }
            TabLevel--;
            SaveRuleList(layer.Rules);
        }

        public void LoadLayer(Layer<T> layer, string givenName = null)
        {
            if (givenName == null)
            {
                string layerName = string.Empty;
                if (!TryReadLine())
                    throw new Exception("Expected Layer at line " + LineCount);
                if (LastLine.Length > 6)
                    layerName = LastLine.Substring(6).Trim();
                layer.Name = layerName;
            }
            else
            {
                layer.Name = givenName;
            }

            TabLevel++;
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("repeat"))
                {
                    string repeatText = LastLine.Substring(8);
                    if (!int.TryParse(repeatText, out layer.Repeats))
                        throw new Exception("Failed to convert '" + repeatText + "' to int at line " + LineCount);
                }
                else if (lowered.StartsWith("autocollapse"))
                {
                    layer.AutoCollapse = true;
                }
                else if (lowered.StartsWith("global"))
                {
                    LoadRuleList(layer.GlobalRules);
                }
                else if (lowered.StartsWith("auto"))
                {
                    LoadRuleList(layer.AutoRules);
                }
                else if (lowered.StartsWith("struct"))
                {
                    LoadStructureList(layer.Structures);
                }
                else
                {
                    UsedLastLine = false;
                    break;
                }
            }
            TabLevel--;
            LoadRuleList(layer.Rules);
        }

        public void SaveRuleList(List<Rule<T>> list)
        {
            TabLevel++;
            foreach (Rule<T> rule in list)
                SaveRule(rule);
            TabLevel--;
        }

        public void LoadRuleList(List<Rule<T>> list)
        {
            TabLevel++;
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                Rule<T> rule = Scout.Recycler.GetRule();
                LoadRule(rule);
                list.Add(rule);
            }
            TabLevel--;
        }

        public void SaveRule(Rule<T> rule, bool skipName = false)
        {
            if (!skipName)
            {
                Write("RULE ");
                WriteLine(rule.Name);
                TabLevel++;
            }

            // write condition
            SaveCondition(rule.Condition);

            // write outcomes
            SaveOutcomeList(rule.Outcomes);

            if (!skipName)
            {
                TabLevel--;
            }
        }

        public void LoadRule(Rule<T> rule)
        {
            string ruleName = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Rule at line " + LineCount);
            if (LastLine.Length > 5)
                ruleName = LastLine.Substring(5).Trim();
            rule.Name = ruleName;

            TabLevel++;
            rule.Condition = LoadCondition();
            LoadOutcomeList(rule.Outcomes);
            TabLevel--;
        }

        public void SaveConditionList(List<ICondition<T>> list, bool tabs = true)
        {
            if (tabs)
                TabLevel++;
            foreach (ICondition<T> cond in list)
                SaveCondition(cond);
            if (tabs)
                TabLevel--;
        }

        public void LoadConditionList(List<ICondition<T>> list, bool tabs = true)
        {
            if (tabs)
                TabLevel++;
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                ICondition<T> cond = LoadCondition();
                list.Add(cond);
            }
            if (tabs)
                TabLevel--;
        }

        public void SaveCondition(ICondition<T> cond)
        {
            Write("? ");
            WriteLine(cond.GetKeyword());

            TabLevel++;
            cond.Save(this);
            TabLevel--;
        }

        public ICondition<T> LoadCondition()
        {
            string keyword = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Condition at line " + LineCount);
            if (LastLine.Length > 2)
                keyword = LastLine.Substring(2).Trim();

            ICondition<T> cond = Scout.Recycler.GetCondition(keyword);
            TabLevel++;
            cond.Load(this);
            TabLevel--;

            return cond;
        }

        public void SaveOutcomeList(List<Outcome<T>> list)
        {
            foreach (Outcome<T> outcome in list)
                SaveOutcome(outcome);
        }

        public void LoadOutcomeList(List<Outcome<T>> list)
        {
            while (TryReadLine())
            {
                Outcome<T> outcome = Scout.Recycler.GetOutcome();
                LoadOutcome(outcome);
                list.Add(outcome);
            }
        }

        public void SaveOutcome(Outcome<T> outcome)
        {
            Write("* ");
            if (outcome.KeywordFloat != null)
                ScoutScript<T>.SaveKeywordFloat(this, outcome.KeywordFloat);
            else if (outcome.KeywordInt != null)
                ScoutScript<T>.SaveKeywordInt(this, outcome.KeywordInt);
            else if (outcome.KeywordString != null)
                ScoutScript<T>.SaveKeywordString(this, outcome.KeywordString);
            else
                throw new Exception("Expected keyword from outcome at line " + LineCount);

            Write(" :");
            Write(outcome.Operation);
            Write(" ");
            if (outcome.ValueFloat != null)
                ScoutScript<T>.SaveKeywordFloat(this, outcome.ValueFloat);
            else if (outcome.ValueInt != null)
                ScoutScript<T>.SaveKeywordInt(this, outcome.ValueInt);
            else if (outcome.ValueString != null)
                ScoutScript<T>.SaveKeywordString(this, outcome.ValueString);
            else
                throw new Exception("Expected value from outcome at line " + LineCount);

            if (outcome.Probability == null)
            {
                WriteLine("");
            }
            else
            {
                Write(" (");
                ScoutScript<T>.SaveKeywordFloat(this, outcome.Probability);
                WriteLine(")");
            }
        }

        public void LoadOutcome(Outcome<T> outcome)
        {
            // read keyword, operation, value, and probability (optional)
            if (!LastLine.StartsWith("* "))
                throw new Exception("Expected Outcome at line " + LineCount);

            int colonPos = LastLine.IndexOf(':');
            if (colonPos == -1)
                throw new Exception("Expected : in Outcome at line " + LineCount);

            string firstHalf = LastLine.Substring(2, colonPos);
            string operation = ParseHelper.ReadToken(LastLine, ' ', colonPos + 1);
            int start = ParseHelper.SkipSpaces(LastLine, colonPos + 1 + operation.Length);

            ScoutScript<T>.LoadKeyword(this, firstHalf, 0, out KeywordReturn<T> keywordReturn);
            outcome.KeywordFloat = keywordReturn.KeywordFloat;
            outcome.KeywordInt = keywordReturn.KeywordInt;
            outcome.KeywordString = keywordReturn.KeywordString;

            outcome.Operation = operation;

            ScoutScript<T>.LoadKeyword(this, LastLine, start, out keywordReturn);
            outcome.ValueFloat = keywordReturn.KeywordFloat;
            outcome.ValueInt = keywordReturn.KeywordInt;
            outcome.ValueString = keywordReturn.KeywordString;

            outcome.Probability = null;
            while (start < LastLine.Length && LastLine[start] != '(')
                start++;
            if (start < LastLine.Length)
            {
                start++; // move past the (
                string probText = ParseHelper.ReadToken(LastLine, ')', start);
                if (!string.IsNullOrEmpty(probText))
                {
                    ScoutScript<T>.LoadKeyword(this, probText, 0, out keywordReturn);
                    if (keywordReturn.KeywordFloat != null)
                        outcome.Probability = keywordReturn.KeywordFloat;
                }
            }
        }

        public void SaveStructureList(List<Structure<T>> list)
        {
            foreach (Structure<T> structure in list)
                SaveStructure(structure);
        }

        public void LoadStructureList(List<Structure<T>> list)
        {
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                Structure<T> structure = Scout.Recycler.GetStructure();
                LoadStructure(structure);
                list.Add(structure);
            }
        }

        public void SaveStructure(Structure<T> structure)
        {
            Write("STRUCT ");
            WriteLine(structure.Name);
            TabLevel++;

            if (structure.MinPoints != 1 || structure.MaxPoints != 1)
            {
                Write("POINTS ");
                Write(structure.MinPoints.ToString());
                Write("-");
                WriteLine(structure.MaxPoints.ToString());
            }
            if (structure.Repeats != 0)
            {
                Write("REPEATS ");
                WriteLine(structure.Repeats.ToString());
            }
            if (structure.Rarity != 1)
            {
                Write("RARITY ");
                WriteLine(structure.Rarity.ToString());
            }

            // write condition
            SaveCondition(structure.Condition);

            // write blocks
            SaveStructureBlockList(structure.Blocks);

            TabLevel--;
        }

        public void LoadStructure(Structure<T> structure)
        {
            string ruleName = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Structure at line " + LineCount);
            if (LastLine.Length > 7)
                ruleName = LastLine.Substring(LastLine.IndexOf(' ') + 1).Trim();
            structure.Name = ruleName;
            structure.MinPoints = 1;
            structure.MaxPoints = 1;
            structure.Rarity = 1;
            structure.Repeats = 0;

            TabLevel++;
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("repeat"))
                {
                    string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    if (!int.TryParse(repeatText, out structure.Repeats))
                        throw new Exception("Failed to convert '" + repeatText + "' to int at line " + LineCount);
                }
                else if (lowered.StartsWith("rarity"))
                {
                    string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    if (!double.TryParse(repeatText, out structure.Rarity))
                        throw new Exception("Failed to convert '" + repeatText + "' to double at line " + LineCount);
                }
                else if (lowered.StartsWith("point"))
                {
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    string minRange = ParseHelper.ReadToken(rangeText, '-', 0);
                    string maxRange = rangeText.Substring(rangeText.IndexOf('-') + 1);
                    if (!int.TryParse(minRange, out structure.MinPoints))
                        throw new Exception("Expected numerical Min Points at line " + LineCount);
                    if (!int.TryParse(maxRange, out structure.MaxPoints))
                        throw new Exception("Expected numerical Max Points at line " + LineCount);
                }
                else
                {
                    UsedLastLine = false;
                    break;
                }
            }

            structure.Condition = LoadCondition();
            LoadStructureBlockList(structure.Blocks);
            TabLevel--;
        }

        public void SaveStructureBlockList(List<StructureBlock<T>> list)
        {
            foreach (StructureBlock<T> block in list)
                SaveStructureBlock(block);
        }

        public void LoadStructureBlockList(List<StructureBlock<T>> list)
        {
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                StructureBlock<T> block = Scout.Recycler.GetStructureBlock();
                LoadStructureBlock(block);
                list.Add(block);
            }
        }

        public void SaveStructureBlock(StructureBlock<T> block)
        {
            Write("BLOCK ");
            WriteLine(block.Name);
            TabLevel++;

            if (block.Beam)
            {
                WriteLine("BEAM");
                TabLevel++;

                if (block.BeamTargetCondition != null)
                {
                    SaveCondition(block.BeamTargetCondition);
                }

                Write("DISTANCE ");
                Write(block.BeamMinDistance.ToString());
                Write("-");
                WriteLine(block.BeamMaxDistance.ToString());

                if (block.BeamInterval != 1)
                {
                    Write("INTERVAL ");
                    WriteLine(block.BeamInterval.ToString());
                }

                if (block.BeamPathfinding != StructureBlock<T>.BEAM_PATHFINDING_SIMPLE)
                {
                    Write("PATHING ");
                    WriteLine(block.BeamPathfinding);
                }

                if (block.BeamWanderChance != 0)
                {
                    WriteLine("WANDER");
                    TabLevel++;

                    if (block.BeamWanderCondition != null)
                    {
                        SaveCondition(block.BeamWanderCondition);
                    }

                    Write("CHANCE ");
                    WriteLine(block.BeamWanderChance.ToString());

                    if (block.BeamWanderRepeats != 0)
                    {
                        Write("REPEATS ");
                        WriteLine(block.BeamWanderRepeats.ToString());
                    }

                    TabLevel--;
                }


                TabLevel--;
            }

            // write cells
            SaveStructureCellList(block.Cells);

            TabLevel--;
        }

        public void LoadStructureBlock(StructureBlock<T> block)
        {
            string ruleName = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Block at line " + LineCount);
            if (LastLine.Length > 6)
                ruleName = LastLine.Substring(LastLine.IndexOf(' ') + 1).Trim();
            block.Name = ruleName;
            block.BeamPathfinding = StructureBlock<T>.BEAM_PATHFINDING_SIMPLE;
            block.BeamInterval = 1;
            block.BeamWanderChance = 0;
            block.BeamWanderRepeats = 0;

            TabLevel++;
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("beam"))
                {
                    TabLevel++;

                    while (TryReadLine())
                    {
                        lowered = LastLine.ToLower();
                        if (lowered.StartsWith("? "))
                        {
                            UsedLastLine = false;
                            block.BeamTargetCondition = LoadCondition();
                        }
                        else if (lowered.StartsWith("distance"))
                        {
                            string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                            string minRange = ParseHelper.ReadToken(rangeText, '-', 0);
                            string maxRange = rangeText.Substring(rangeText.IndexOf('-') + 1);
                            if (!int.TryParse(minRange, out block.BeamMinDistance))
                                throw new Exception("Expected numerical Min Beam Distance at line " + LineCount);
                            if (!int.TryParse(maxRange, out block.BeamMaxDistance))
                                throw new Exception("Expected numerical Max Beam Distance at line " + LineCount);
                        }
                        else if (lowered.StartsWith("interval"))
                        {
                            string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                            if (!int.TryParse(repeatText, out block.BeamInterval))
                                throw new Exception("Failed to convert '" + repeatText + "' to int at line " + LineCount);
                        }
                        else if (lowered.StartsWith("pathing"))
                        {
                            string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                            block.BeamPathfinding = repeatText;
                        }
                        else if (lowered.StartsWith("wander"))
                        {
                            TabLevel++;

                            while (TryReadLine())
                            {
                                lowered = LastLine.ToLower();
                                if (lowered.StartsWith("? "))
                                {
                                    UsedLastLine = false;
                                    block.BeamWanderCondition = LoadCondition();
                                }
                                else if (lowered.StartsWith("chance"))
                                {
                                    string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                                    if (!double.TryParse(repeatText, out block.BeamWanderChance))
                                        throw new Exception("Failed to convert '" + repeatText + "' to double at line " + LineCount);
                                }
                                else if (lowered.StartsWith("repeat"))
                                {
                                    string repeatText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                                    if (!int.TryParse(repeatText, out block.BeamWanderRepeats))
                                        throw new Exception("Failed to convert '" + repeatText + "' to int at line " + LineCount);
                                }
                                else
                                {
                                    UsedLastLine = false;
                                    break;
                                }
                            }

                            TabLevel--;
                        }
                        else
                        {
                            UsedLastLine = false;
                            break;
                        }
                    }

                    TabLevel--;
                }
                else 
                {
                    UsedLastLine = false;
                    break;
                }
            }

            LoadStructureCellList(block.Cells);
            TabLevel--;
        }

        public void SaveStructureCellList(List<StructureCell<T>> list)
        {
            foreach (StructureCell<T> cell in list)
                SaveStructureCell(cell);
        }

        public void LoadStructureCellList(List<StructureCell<T>> list)
        {
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                StructureCell<T> cell = Scout.Recycler.GetStructureCell();
                LoadStructureCell(cell);
                list.Add(cell);
            }
        }

        public void SaveStructureCell(StructureCell<T> cell)
        {
            Write("CELL ");
            WriteLine(cell.Name);
            TabLevel++;

            Write("RADIUS ");
            Write(cell.MinRadius.ToString());
            Write("-");
            WriteLine(cell.Radius.ToString());

            if (cell.MinXOffset != float.MinValue || cell.MaxXOffset != float.MaxValue)
            {
                Write("XRANGE ");
                Write(cell.MinXOffset.ToString());
                Write("-");
                WriteLine(cell.MaxXOffset.ToString());
            }

            if (cell.MinYOffset != float.MinValue || cell.MaxYOffset != float.MaxValue)
            {
                Write("YRANGE ");
                Write(cell.MinYOffset.ToString());
                Write("-");
                WriteLine(cell.MaxYOffset.ToString());
            }

            SaveStructureRuleList(cell.Rules);

            TabLevel--;
        }

        public void LoadStructureCell(StructureCell<T> cell)
        {
            string ruleName = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Cell at line " + LineCount);
            if (LastLine.Length > 5)
                ruleName = LastLine.Substring(LastLine.IndexOf(' ') + 1).Trim();
            cell.Name = ruleName;

            cell.MinXOffset = float.MinValue;
            cell.MaxXOffset = float.MaxValue;

            cell.MinYOffset = float.MinValue;
            cell.MaxYOffset = float.MaxValue;

            TabLevel++;
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("radius"))
                {
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!int.TryParse(minRange, out cell.MinRadius))
                        throw new Exception("Expected numerical Min Radius at line " + LineCount);
                    if (!int.TryParse(maxRange, out cell.Radius))
                        throw new Exception("Expected numerical Max Radius at line " + LineCount);
                }
                else if (lowered.StartsWith("xrange"))
                {
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!float.TryParse(minRange, out cell.MinXOffset))
                        throw new Exception("Expected numerical Min X Range at line " + LineCount);
                    if (!float.TryParse(maxRange, out cell.MaxXOffset))
                        throw new Exception("Expected numerical Max X Range at line " + LineCount);
                }
                else if (lowered.StartsWith("yrange"))
                {
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!float.TryParse(minRange, out cell.MinYOffset))
                        throw new Exception("Expected numerical Min Y Range at line " + LineCount);
                    if (!float.TryParse(maxRange, out cell.MaxYOffset))
                        throw new Exception("Expected numerical Max Y Range at line " + LineCount);
                }
                else
                {
                    UsedLastLine = false;
                    break;
                }
            }

            LoadStructureRuleList(cell.Rules);
            TabLevel--;
        }

        public void SaveStructureRuleList(List<StructureRule<T>> list)
        {
            foreach (StructureRule<T> cell in list)
                SaveStructureRule(cell);
        }

        public void LoadStructureRuleList(List<StructureRule<T>> list)
        {
            while (TryReadLine())
            {
                UsedLastLine = false; // force line to be reused
                StructureRule<T> cell = Scout.Recycler.GetStructureRule();
                LoadStructureRule(cell);
                list.Add(cell);
            }
        }

        public void SaveStructureRule(StructureRule<T> cell)
        {
            Write("RULE ");
            WriteLine(cell.Rule.Name);
            TabLevel++;

            if (cell.MinRadius != null || cell.Radius != null)
            {
                Write("RADIUS ");
                Write(cell.MinRadius.ToString());
                Write("-");
                WriteLine(cell.Radius.ToString());
            }

            if (cell.MinXOffset != null || cell.MaxXOffset != null)
            {
                Write("XRANGE ");
                Write(cell.MinXOffset.ToString());
                Write("-");
                WriteLine(cell.MaxXOffset.ToString());
            }

            if (cell.MinYOffset != null || cell.MaxYOffset != null)
            {
                Write("YRANGE ");
                Write(cell.MinYOffset.ToString());
                Write("-");
                WriteLine(cell.MaxYOffset.ToString());
            }

            SaveRule(cell.Rule, skipName: true);

            TabLevel--;
        }

        public void LoadStructureRule(StructureRule<T> cell)
        {
            string ruleName = string.Empty;
            if (!TryReadLine())
                throw new Exception("Expected Structure Rule at line " + LineCount);
            if (LastLine.Length > 5)
                ruleName = LastLine.Substring(LastLine.IndexOf(' ') + 1).Trim();
            cell.Rule.Name = ruleName;

            cell.MinRadius = null;
            cell.Radius = null;

            cell.MinXOffset = null;
            cell.MaxXOffset = null;

            cell.MinYOffset = null;
            cell.MaxYOffset = null;

            TabLevel++;
            while (TryReadLine())
            {
                string lowered = LastLine.ToLower();
                if (lowered.StartsWith("radius"))
                {
                    cell.MinRadius = 0;
                    cell.Radius = 0;
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!int.TryParse(minRange, out int minRadius))
                        throw new Exception("Expected numerical Min Radius at line " + LineCount);
                    cell.MinRadius = minRadius;

                    if (!int.TryParse(maxRange, out int radius))
                        throw new Exception("Expected numerical Max Radius at line " + LineCount);
                    cell.Radius = radius;
                }
                else if (lowered.StartsWith("xrange"))
                {
                    cell.MinXOffset = float.MinValue;
                    cell.MaxXOffset = float.MaxValue;
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!float.TryParse(minRange, out float minoff))
                        throw new Exception("Expected numerical Min X Range at line " + LineCount);
                    cell.MinXOffset = minoff;

                    if (!float.TryParse(maxRange, out float maxoff))
                        throw new Exception("Expected numerical Max X Range at line " + LineCount);
                    cell.MaxXOffset = maxoff;
                }
                else if (lowered.StartsWith("yrange"))
                {
                    cell.MinYOffset = float.MinValue;
                    cell.MaxYOffset = float.MaxValue;
                    string rangeText = LastLine.Substring(LastLine.IndexOf(' ') + 1);
                    ParseHelper.ReadRange(rangeText, out string minRange, out string maxRange);
                    if (!float.TryParse(minRange, out float minoff))
                        throw new Exception("Expected numerical Min Y Range at line " + LineCount);
                    cell.MinYOffset = minoff;

                    if (!float.TryParse(maxRange, out float maxoff))
                        throw new Exception("Expected numerical Max Y Range at line " + LineCount);
                    cell.MaxYOffset = maxoff;
                }
                else
                {
                    UsedLastLine = false;
                    break;
                }
            }

            cell.Rule.Condition = LoadCondition();
            LoadOutcomeList(cell.Rule.Outcomes);
            TabLevel--;
        }
    }
}
