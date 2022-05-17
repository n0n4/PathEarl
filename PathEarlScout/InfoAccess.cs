using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class InfoAccess<T> where T : ITileInfo
    {
        public Dictionary<string, Func<T, int>> GetInts = new Dictionary<string, Func<T, int>>();
        public Dictionary<string, Action<T, int>> SetInts = new Dictionary<string, Action<T, int>>();

        public Dictionary<string, Func<T, float>> GetFloats = new Dictionary<string, Func<T, float>>();
        public Dictionary<string, Action<T, float>> SetFloats = new Dictionary<string, Action<T, float>>();

        public Dictionary<string, Func<T, string>> GetStrings = new Dictionary<string, Func<T, string>>();
        public Dictionary<string, Action<T, string>> SetStrings = new Dictionary<string, Action<T, string>>();
    }
}
