using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class Layer<T> where T : ITileInfo
    {
        public string Name;
        public List<Rule<T>> GlobalRules;
        public List<Rule<T>> AutoRules;
        public List<Rule<T>> Rules;
        public int Repeats = 0;
        public bool AutoCollapse = false;

        public Layer(ScoutRecycler<T> recycler) 
        {
            GlobalRules = recycler.GetRuleList();
            AutoRules = recycler.GetRuleList();
            Rules = recycler.GetRuleList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach (Rule<T> rule in GlobalRules)
            {
                recycler.ReturnRule(rule);
            }
            GlobalRules.Clear();

            foreach (Rule<T> rule in AutoRules)
            {
                recycler.ReturnRule(rule);
            }
            AutoRules.Clear();

            foreach(Rule<T> rule in Rules)
            {
                recycler.ReturnRule(rule);
            }
            Rules.Clear();
        }
    }
}
