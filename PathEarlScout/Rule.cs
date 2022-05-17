using PathEarlCore;
using PathEarlScout.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class Rule<T> where T : ITileInfo
    {
        public string Name;
        public ICondition<T> Condition;

        public List<Outcome<T>> Outcomes;

        public Rule(ScoutRecycler<T> recycler)
        {
            Outcomes = recycler.GetOutcomeList();
        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            foreach (Outcome<T> outcome in Outcomes)
                recycler.ReturnOutcome(outcome);
            Outcomes.Clear();

            recycler.ReturnCondition(Condition);
            Condition = null;
        }
    }
}
