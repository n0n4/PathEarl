using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlViz
{
    public class Bitmap
    {
        public int Width = 0;
        public int Height = 0;
        public byte[,] R = null;
        public byte[,] G = null;
        public byte[,] B = null;
        public byte[,] A = null;

        public float SmallestX;
        public float SmallestY;
        public float LargestX;
        public float LargestY;
        public int Resolution;

        public Bitmap(int x, int y)
        {
            Width = x;
            Height = y;
            R = new byte[x, y];
            G = new byte[x, y];
            B = new byte[x, y];
            A = new byte[x, y];
        }

        public void SetPixel(int x, int y, byte[] col)
        {
            SetPixel(x, y, col[0], col[1], col[2], col[3]);
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            R[x, y] = r;
            G[x, y] = g;
            B[x, y] = b;
            A[x, y] = a;
        }

        public void SetPixel(int x, int y, byte[] col, int radius)
        {
            for (int i = -1 * radius; i <= radius; i++)
            {
                for (int o = -1 * radius; o <= radius; o++)
                {
                    int ix = x + i;
                    int oy = y + o;

                    if (ix >= 0 && oy >= 0 && ix < Width && oy < Height)
                    {
                        SetPixel(ix, oy, col);
                    }
                }
            }
        }

        public void Fill(byte[] col)
        {
            Fill(col[0], col[1], col[2], col[3]);
        }

        public void Fill(byte r, byte g, byte b, byte a)
        {
            int i = 0;
            while (i < Width)
            {
                int o = 0;
                while (o < Height)
                {
                    SetPixel(i, o, r, g, b, a);
                    o++;
                }
                i++;
            }
        }

        public void DrawLine(int x, int y, int x2, int y2, byte[] col)
        {
            DrawLine(x, y, x2, y2, col[0], col[1], col[2], col[3]);
        }

        public void DrawLine(int x, int y, int x2, int y2, byte r, byte g, byte b, byte a)
        {
            // this is an implementation of the Bresenham line algorithm
            int w = x2 - x;
            int h = y2 - y;
            double deltax = (double)w;
            int i = y;
            if (deltax == 0)
            {
                // special case where delta x is 0
                // must not use standard line algorithm because otherwise we'd get divide by zero error
                // when attempting to calculate the slope of this line.
                // instead, just start at x,y and draw a line to x,y2, which is trivial
                while (i != y2)
                {
                    if (x >= 0 && x < Width && i >= 0 && i < Height)
                        SetPixel(x, i, r, g, b, a);
                    i += Math.Sign(h);
                }
                return; // avoid divide by 0 ahead
            }
            double deltay = (double)h;
            double deltaerr = Math.Abs(deltay / deltax); // slope
            double error = 0;
            int o = x;
            while (o != x2)
            {
                if (o >= 0 && o < Width && i >= 0 && i < Height)
                    SetPixel(o, i, r, g, b, a);
                error = error + deltaerr;
                if (error >= 0.5)
                {
                    i += Math.Sign(deltay);
                    error = error - 1.0;
                }
                o += Math.Sign(w); // progress closer to our final x position
            }
        }

        public System.Drawing.Bitmap ToDrawingBitmap()
        {
            System.Drawing.Bitmap drawing = new System.Drawing.Bitmap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    drawing.SetPixel(i, o, System.Drawing.Color.FromArgb(A[i, o], R[i, o], G[i, o], B[i, o]));
                }
            }
            return drawing;
        }
    }
}
