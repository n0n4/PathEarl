using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionAdjacent<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "adjacent";
        //       from, to
        public ICondition<T> Condition;
        public int Min = 1;
        public int Max = -1;
        public int StartRange = 0;
        public int StopRange = 1;

        private Dictionary<int, int> SearchDict = new Dictionary<int, int>();
        private List<int> Searches = new List<int>();

        public ConditionAdjacent(ScoutRecycler<T> recycler)
        {

        }

        public void Clear(ScoutRecycler<T> recycler)
        {
            recycler.ReturnCondition(Condition);
            Condition = null;
        }

        public string GetKeyword()
        {
            return Keyword;
        }

        public bool Evaluate(KeywordContext<T> context)
        {
            Searches.Clear();
            SearchDict.Clear();

            context.AddAlreadyHitStack();
            Tile<T> tile = context.Tiles[context.Count - 1];
            int id = tile.Id;

            int count = 0;
            foreach (var conn in context.Map.Nodes[id])
            {
                Searches.Add(conn.Key);
                SearchDict.Add(conn.Key, 1);
            }

            for (int i = 0; i < Searches.Count; i++)
            {
                int connId = Searches[i];
                int connRange = SearchDict[connId];

                // add next conns if we're within range
                if (connRange < StopRange)
                {
                    foreach (var nextConn in context.Map.Nodes[connId])
                    {
                        if (context.IsAlreadyHit(nextConn.Key) || SearchDict.ContainsKey(nextConn.Key))
                            continue;

                        Searches.Add(nextConn.Key);
                        SearchDict.Add(nextConn.Key, connRange + 1);
                    }
                }

                if (context.IsAlreadyHit(connId))
                    continue;
                context.AddHit(connId);

                if (connRange < StartRange || connRange > StopRange)
                    continue;

                context.Add(connId);
                if (Condition.Evaluate(context))
                    count++;
                context.Remove();

                if (count >= Min && Max == -1)
                {
                    context.RemoveAlreadyHitStack();
                    return true;
                }

                if (count > Max && Max != -1)
                {
                    context.RemoveAlreadyHitStack();
                    return false;
                }
            }

            context.RemoveAlreadyHitStack();
            return (count >= Min && count <= Max);
        }

        public void Save(ScoutSerializer<T> serializer)
        {
            if (Min != 1)
            {
                serializer.Write("MIN ");
                serializer.WriteLine(Min.ToString());
            }
            if (Max != -1)
            {
                serializer.Write("MAX ");
                serializer.WriteLine(Max.ToString());
            }
            if (StartRange != 0 || StopRange != 1)
            {
                serializer.Write("RANGE ");
                serializer.Write(StartRange.ToString());
                serializer.Write("-");
                serializer.WriteLine(StopRange.ToString());
            }

            serializer.SaveCondition(Condition);
        }

        public void Load(ScoutSerializer<T> serializer)
        {
            while (serializer.TryReadLine())
            {
                string lowered = serializer.LastLine.ToLower();
                if (lowered.StartsWith("min"))
                {
                    string minText = serializer.LastLine.Substring(4);
                    if (!int.TryParse(minText, out Min))
                        throw new Exception("Expected numerical Min at line " + serializer.LineCount);
                }
                else if (lowered.StartsWith("max"))
                {
                    string maxText = serializer.LastLine.Substring(4);
                    if (!int.TryParse(maxText, out Max))
                        throw new Exception("Expected numerical Max at line " + serializer.LineCount);
                }
                else if (lowered.StartsWith("range"))
                {
                    string rangeText = serializer.LastLine.Substring(6);
                    string minRange = ParseHelper.ReadToken(rangeText, '-', 0);
                    string maxRange = rangeText.Substring(rangeText.IndexOf('-') + 1);
                    if (!int.TryParse(minRange, out StartRange))
                        throw new Exception("Expected numerical Min Range at line " + serializer.LineCount);
                    if (!int.TryParse(maxRange, out StopRange))
                        throw new Exception("Expected numerical Max Range at line " + serializer.LineCount);
                }
                else
                {
                    serializer.UsedLastLine = false;
                    break;
                }
            }

            Condition = serializer.LoadCondition();
        }
    }
}
