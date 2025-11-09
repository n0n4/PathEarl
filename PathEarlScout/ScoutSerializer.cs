using PathEarlCore;
using PathEarlScout.Conditions;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
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
                if (lowered.StartsWith("repeats"))
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

        public void SaveRule(Rule<T> rule)
        {
            Write("RULE ");
            WriteLine(rule.Name);
            TabLevel++;

            // write condition
            SaveCondition(rule.Condition);

            // write outcomes
            SaveOutcomeList(rule.Outcomes);
            
            TabLevel--;
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
    }
}
