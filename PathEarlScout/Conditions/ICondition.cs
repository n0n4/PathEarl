using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public interface ICondition<T> where T : ITileInfo
    {
        string GetKeyword(); 
        void Clear(ScoutRecycler<T> recycler);
        bool Evaluate(KeywordContext<T> context);
        void Save(ScoutSerializer<T> serializer);
        void Load(ScoutSerializer<T> serializer);
    }
}
