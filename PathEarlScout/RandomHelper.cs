using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public static class RandomHelper
    {
        public static void Shuffle<T>(Random random, List<T> list)
        {
            int j = 0;
            int count = list.Count;
            for (int i = 0; i < count - 1; i++)
            {
                j = random.Next(i, count);
                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }
    }
}
