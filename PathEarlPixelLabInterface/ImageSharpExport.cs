using PathEarlViz;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PathEarlPixelLabInterface
{
    public static class ImageSharpExport
    {
        public static void Save(string filepath, Bitmap bitmap)
        {
            // use imagesharp to convert this image and save it
            using (Image<Rgba32> img = new Image<Rgba32>(bitmap.Width, bitmap.Height))
            {
                int i = 0;
                while (i < bitmap.Width)
                {
                    int o = 0;
                    while (o < bitmap.Height)
                    {
                        img[i, o] = new Rgba32(
                            bitmap.R[i,o],
                            bitmap.G[i, o],
                            bitmap.B[i, o],
                            bitmap.A[i, o]);
                        o++;
                    }
                    i++;
                }
                img.Save(filepath);
            }
        }
    }
}
