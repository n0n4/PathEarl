using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class Layer<T> where T : ITileInfo
    {
        public string Name;
        public List<Rule<T>> Rules;
        public int Repeats = 0;

        public Layer(ScoutRecycler<T> recycler) 
        {
            Rules = recycler.GetRuleList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach(Rule<T> rule in Rules)
            {
                recycler.ReturnRule(rule);
            }
            Rules.Clear();
        }
    }
}
