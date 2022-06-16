using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Conditions
{
    public class ConditionDirection<T> : ICondition<T> where T : ITileInfo
    {
        public const string Keyword = "direction";
        public ICondition<T> Condition;

        public static float MaxDistance = 15; // must be within 15 degrees
        public float Angle = 0;
        public ConditionDirection(ScoutRecycler<T> recycler)
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
            Tile<T> tile = context.Tiles[context.Count - 1];
            int id = tile.Id;

            int closestId = -1;
            float closestDiff = float.MaxValue;
            foreach (var conn in context.Map.Nodes[id])
            {
                float angle = context.Map.GetAngleDeg(id, conn.Key);
                float small = Angle;
                if (angle < Angle)
                {
                    small = angle;
                    angle = Angle;
                }

                float dist1 = angle - small;
                float dist2 = small - (angle - 360);
                if (dist2 < dist1)
                    dist1 = dist2;

                if (dist1 <= MaxDistance && dist1 < closestDiff)
                {
                    closestDiff = dist1;
                    closestId = conn.Key;
                }
            }

            bool valid = false;
            if (closestId != -1)
            {
                context.Add(closestId);
                valid = Condition.Evaluate(context);
                context.Remove();
            }

            return valid;
        }

        public void Save(ScoutSerializer<T> serializer)
        {
            serializer.Write("ANGLE ");
            serializer.WriteLine(Angle.ToString());

            serializer.SaveCondition(Condition);
        }

        public void Load(ScoutSerializer<T> serializer)
        {
            while (serializer.TryReadLine())
            {
                string lowered = serializer.LastLine.ToLower();
                if (lowered.StartsWith("angle"))
                {
                    string angleText = serializer.LastLine.Substring(4);
                    if (!float.TryParse(angleText, out Angle))
                        throw new Exception("Expected numerical Angle at line " + serializer.LineCount);
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
