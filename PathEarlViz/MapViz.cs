using PathEarlCore;
using System;
using System.Collections.Generic;

namespace PathEarlViz
{
    public static class MapViz<T> where T : ITileInfo
    {
        public static Bitmap DrawMap(Map<T> m, int resolution,
            byte[] backcolor, byte[] linecolor, Func<Map<T>, int, byte[]> coloration)
        {
            

            // first we need to determine the min and max x,y
            float smallestx = float.MaxValue;
            float smallesty = float.MaxValue;
            float largestx = float.MinValue;
            float largesty = float.MinValue;

            foreach (int id in m.Nodes.Keys)
            {
                float x = m.NodeX[id];
                float y = m.NodeY[id];
                if (x < smallestx)
                {
                    smallestx = x;
                }
                if (y < smallesty)
                {
                    smallesty = y;
                }
                if (x > largestx)
                {
                    largestx = x;
                }
                if (y > largesty)
                {
                    largesty = y;
                }
            }

            // given these bounds, we can create the bitmap
            float width = largestx - smallestx;
            float height = largesty - smallesty;
            int iwidth = (int)(width * resolution) + 1;
            int iheight = (int)(height * resolution) + 1;
            Bitmap b = new Bitmap(iwidth, iheight);
            b.SmallestX = smallestx;
            b.SmallestY = smallesty;
            b.LargestX = largestx;
            b.LargestY = largesty;
            b.Resolution = resolution;

            b.Fill(backcolor); // give it a backdrop

            // now draw every point and their connections

            // draw connections first
            foreach (int id in m.Nodes.Keys)
            {
                float x = m.NodeX[id];
                float y = m.NodeY[id];
                int relx = FindRelativePosition(x, smallestx, resolution);
                int rely = FindRelativePosition(y, smallesty, resolution);
                foreach (int tid in m.Nodes[id].Keys)
                {
                    float tx = m.NodeX[tid];
                    float ty = m.NodeY[tid];
                    int trelx = FindRelativePosition(tx, smallestx, resolution);
                    int trely = FindRelativePosition(ty, smallesty, resolution);
                    b.DrawLine(relx, rely, trelx, trely, linecolor);
                }
            }

            // After the connections are done, go back and place the dots
            foreach (int id in m.Nodes.Keys)
            {
                float x = m.NodeX[id];
                float y = m.NodeY[id];
                int relx = FindRelativePosition(x, smallestx, resolution);
                int rely = FindRelativePosition(y, smallesty, resolution);

                byte[] col = coloration(m, id);

                b.SetPixel(relx, rely, col, resolution/2);
            }

            return b;
        }

        public static Bitmap DrawPath(Bitmap B, Map<T> m, List<int> path, byte[] linecolor, int yoff)
        {
            if (path.Count <= 1)
                return B;

            int lastid = path[0];
            for (int i = 1; i < path.Count; i++)
            {
                int newid = path[i];
                // draw a line from lastid to newid

                float lx = m.NodeX[lastid];
                float ly = m.NodeY[lastid];
                int lrelx = FindRelativePosition(lx, B.SmallestX, B.Resolution);
                int lrely = FindRelativePosition(ly, B.SmallestY, B.Resolution);

                float nx = m.NodeX[newid];
                float ny = m.NodeY[newid];
                int nrelx = FindRelativePosition(nx, B.SmallestX, B.Resolution);
                int nrely = FindRelativePosition(ny, B.SmallestY, B.Resolution);
                B.DrawLine(lrelx, lrely + yoff, nrelx, nrely + yoff, linecolor);

                B.DrawLine(lrelx - 1, lrely + yoff, nrelx - 1, nrely + yoff, linecolor);
                B.DrawLine(lrelx, lrely - 1 + yoff, nrelx, nrely - 1 + yoff, linecolor);
                B.DrawLine(lrelx - 1, lrely - 1 + yoff, nrelx - 1, nrely - 1 + yoff, linecolor);

                B.DrawLine(lrelx - 2, lrely - 1 + yoff, nrelx - 2, nrely - 1 + yoff, linecolor);
                B.DrawLine(lrelx - 1, lrely - 2 + yoff, nrelx - 1, nrely - 2 + yoff, linecolor);
                B.DrawLine(lrelx - 2, lrely - 2 + yoff, nrelx - 2, nrely - 2 + yoff, linecolor);

                lastid = newid;
            }

            return B;
        }

        public static Bitmap DrawFlowfield(Bitmap B, Map<T> m, Dictionary<int, int> flowfield, byte[] linecolor, int yoff)
        {
            if (flowfield.Count <= 1)
                return B;

            foreach (var kvp in flowfield)
            {
                int lastid = kvp.Key;
                int newid = kvp.Value;

                float lx = m.NodeX[lastid];
                float ly = m.NodeY[lastid];
                int lrelx = FindRelativePosition(lx, B.SmallestX, B.Resolution);
                int lrely = FindRelativePosition(ly, B.SmallestY, B.Resolution);

                float nx = m.NodeX[newid];
                float ny = m.NodeY[newid];
                int nrelx = FindRelativePosition(nx, B.SmallestX, B.Resolution);
                int nrely = FindRelativePosition(ny, B.SmallestY, B.Resolution);
                B.DrawLine(lrelx, lrely + yoff, nrelx, nrely + yoff, linecolor);

                B.DrawLine(lrelx - 1, lrely + yoff, nrelx - 1, nrely + yoff, linecolor);
                B.DrawLine(lrelx, lrely - 1 + yoff, nrelx, nrely - 1 + yoff, linecolor);
                B.DrawLine(lrelx - 1, lrely - 1 + yoff, nrelx - 1, nrely - 1 + yoff, linecolor);

                B.DrawLine(lrelx - 2, lrely - 1 + yoff, nrelx - 2, nrely - 1 + yoff, linecolor);
                B.DrawLine(lrelx - 1, lrely - 2 + yoff, nrelx - 1, nrely - 2 + yoff, linecolor);
                B.DrawLine(lrelx - 2, lrely - 2 + yoff, nrelx - 2, nrely - 2 + yoff, linecolor);
            }

            return B;
        }

        public static Bitmap DrawSearch(Bitmap B, Map<T> m, float max, Dictionary<int, Map<T>.SearchResult> search, byte[] linecolor, int yoff)
        {
            if (search.Count <= 1)
                return B;

            byte[] col = new byte[] { linecolor[0], linecolor[1], linecolor[2], linecolor[3] };

            foreach (var kvp in search)
            {
                int lastid = kvp.Key;
                int newid = kvp.Value.Next;
                float dist = kvp.Value.Distance;

                if (search.ContainsKey(newid))
                {
                    float lx = m.NodeX[lastid];
                    float ly = m.NodeY[lastid];
                    int lrelx = FindRelativePosition(lx, B.SmallestX, B.Resolution);
                    int lrely = FindRelativePosition(ly, B.SmallestY, B.Resolution);

                    float nx = m.NodeX[newid];
                    float ny = m.NodeY[newid];
                    int nrelx = FindRelativePosition(nx, B.SmallestX, B.Resolution);
                    int nrely = FindRelativePosition(ny, B.SmallestY, B.Resolution);

                    float closeness = 1f - (dist / max) * 0.5f;

                    col[0] = (byte)(linecolor[0] * closeness);
                    col[1] = (byte)(linecolor[1] * closeness);
                    col[2] = (byte)(linecolor[2] * closeness);
                    col[3] = linecolor[3];

                    B.DrawLine(lrelx, lrely + yoff, nrelx, nrely + yoff, col);

                    B.DrawLine(lrelx - 1, lrely + yoff, nrelx - 1, nrely + yoff, col);
                    B.DrawLine(lrelx, lrely - 1 + yoff, nrelx, nrely - 1 + yoff, col);
                    B.DrawLine(lrelx - 1, lrely - 1 + yoff, nrelx - 1, nrely - 1 + yoff, col);

                    B.DrawLine(lrelx - 2, lrely - 1 + yoff, nrelx - 2, nrely - 1 + yoff, col);
                    B.DrawLine(lrelx - 1, lrely - 2 + yoff, nrelx - 1, nrely - 2 + yoff, col);
                    B.DrawLine(lrelx - 2, lrely - 2 + yoff, nrelx - 2, nrely - 2 + yoff, col);
                } 
                else
                {
                    float closeness = 1f - (dist / max) * 0.5f;

                    col[0] = (byte)(linecolor[0] * closeness);
                    col[1] = (byte)(linecolor[1] * closeness);
                    col[2] = (byte)(linecolor[2] * closeness);
                    col[3] = linecolor[3];

                    float lx = m.NodeX[lastid];
                    float ly = m.NodeY[lastid];
                    int lrelx = FindRelativePosition(lx, B.SmallestX, B.Resolution);
                    int lrely = FindRelativePosition(ly, B.SmallestY, B.Resolution);

                    B.SetPixel(lrelx, lrely, col, 2);
                }
            }

            return B;
        }

        public static int FindRelativePosition(float pos, float min, int res)
        {
            pos -= min;
            return (int)(pos * res);
        }

    }
}
