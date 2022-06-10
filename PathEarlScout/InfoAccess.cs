using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout
{
    public class InfoAccess<T> where T : ITileInfo
    {
        public Dictionary<string, Func<Tile<T>, int>> GetInts = new Dictionary<string, Func<Tile<T>, int>>();
        public Dictionary<string, Action<Tile<T>, int>> SetInts = new Dictionary<string, Action<Tile<T>, int>>();

        public Dictionary<string, Func<Tile<T>, float>> GetFloats = new Dictionary<string, Func<Tile<T>, float>>();
        public Dictionary<string, Action<Tile<T>, float>> SetFloats = new Dictionary<string, Action<Tile<T>, float>>();

        public Dictionary<string, Func<Tile<T>, string>> GetStrings = new Dictionary<string, Func<Tile<T>, string>>();
        public Dictionary<string, Action<Tile<T>, string>> SetStrings = new Dictionary<string, Action<Tile<T>, string>>();

        private Dictionary<string, int> OutputInts = new Dictionary<string, int>();
        private Dictionary<string, float> OutputFloats = new Dictionary<string, float>();
        private Dictionary<string, string> OutputStrings = new Dictionary<string, string>();

        private Dictionary<string, Func<Tile<T>, int>> GlobalGetInts = new Dictionary<string, Func<Tile<T>, int>>();
        private Dictionary<string, Action<Tile<T>, int>> GlobalSetInts = new Dictionary<string, Action<Tile<T>, int>>();

        private Dictionary<string, Func<Tile<T>, float>> GlobalGetFloats = new Dictionary<string, Func<Tile<T>, float>>();
        private Dictionary<string, Action<Tile<T>, float>> GlobalSetFloats = new Dictionary<string, Action<Tile<T>, float>>();

        private Dictionary<string, Func<Tile<T>, string>> GlobalGetStrings = new Dictionary<string, Func<Tile<T>, string>>();
        private Dictionary<string, Action<Tile<T>, string>> GlobalSetStrings = new Dictionary<string, Action<Tile<T>, string>>();

        public Map<T> Map;

        public InfoAccess()
        {
            // add default accessors
            GetInts.Add("id", (tile) => { return tile.Id; });
            SetInts.Add("id", (tile, value) => { throw new Exception("Cannot set id"); });

            GetInts.Add("blocking", (tile) => { return (int)tile.Blocks; });
            SetInts.Add("blocking", SetBlocking);

            GetInts.Add("block", (tile) => { return 0; });
            SetInts.Add("block", AddBlock);

            GetInts.Add("unblock", (tile) => { return 0; });
            SetInts.Add("unblock", Unblock);

            GetFloats.Add("x", (tile) => { return tile.X; });
            SetFloats.Add("x", (tile, value) => { throw new Exception("Cannot set x"); });

            GetFloats.Add("y", (tile) => { return tile.Y; });
            SetFloats.Add("y", (tile, value) => { throw new Exception("Cannot set y"); });
        }

        public void LoadOutputs(Dictionary<string, int> ints, Dictionary<string, float> floats, Dictionary<string, string> strings)
        {
            OutputInts = ints;
            OutputFloats = floats;
            OutputStrings = strings;
        }

        public bool TryGetFloatGet(string owner, string keyword, out Func<Tile<T>, float> func)
        {
            if (owner == "floats")
            {
                if (!GlobalGetFloats.TryGetValue(keyword, out func))
                {
                    func = (tile) => { return GetGlobalFloat(keyword); };
                    GlobalGetFloats.Add(keyword, func);
                }
                return true;
            }
            if (GetFloats.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public bool TryGetFloatSet(string owner, string keyword, out Action<Tile<T>, float> func)
        {
            if (owner == "floats")
            {
                if (!GlobalSetFloats.TryGetValue(keyword, out func))
                {
                    func = (tile, value) => { SetGlobalFloat(keyword, value); };
                    GlobalSetFloats.Add(keyword, func);
                }
                return true;
            }
            if (SetFloats.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public bool TryGetIntGet(string owner, string keyword, out Func<Tile<T>, int> func)
        {
            if (owner == "ints")
            {
                if (!GlobalGetInts.TryGetValue(keyword, out func))
                {
                    func = (tile) => { return GetGlobalInt(keyword); };
                    GlobalGetInts.Add(keyword, func);
                }
                return true;
            }
            if (GetInts.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public bool TryGetIntSet(string owner, string keyword, out Action<Tile<T>, int> func)
        {
            if (owner == "ints")
            {
                if (!GlobalSetInts.TryGetValue(keyword, out func))
                {
                    func = (tile, value) => { SetGlobalInt(keyword, value); };
                    GlobalSetInts.Add(keyword, func);
                }
                return true;
            }
            if (SetInts.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public bool TryGetStringGet(string owner, string keyword, out Func<Tile<T>, string> func)
        {
            if (owner == "strings")
            {
                if (!GlobalGetStrings.TryGetValue(keyword, out func))
                {
                    func = (tile) => { return GetGlobalString(keyword); };
                    GlobalGetStrings.Add(keyword, func);
                }
                return true;
            }
            if (GetStrings.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public bool TryGetStringSet(string owner, string keyword, out Action<Tile<T>, string> func)
        {
            if (owner == "strings")
            {
                if (!GlobalSetStrings.TryGetValue(keyword, out func))
                {
                    func = (tile, value) => { SetGlobalString(keyword, value); };
                    GlobalSetStrings.Add(keyword, func);
                }
                return true;
            }
            if (SetStrings.TryGetValue(keyword, out func))
                return true;
            return false;
        }

        public void SetBlocking(Tile<T> tile, int blocks)
        {
            Map.Blocks[tile.Id] = (EMapLayer)blocks;
        }

        public void AddBlock(Tile<T> tile, int block)
        {
            Map.Block(tile.Id, (EMapLayer)block);
        }

        public void Unblock(Tile<T> tile, int block)
        {
            Map.Unblock(tile.Id, (EMapLayer)block);
        }

        public int GetGlobalInt(string name)
        {
            if (OutputInts.TryGetValue(name, out int value))
                return value;
            return 0;
        }

        public void SetGlobalInt(string name, int value)
        {
            OutputInts[name] = value;
        }

        public float GetGlobalFloat(string name)
        {
            if (OutputFloats.TryGetValue(name, out float value))
                return value;
            return 0;
        }

        public void SetGlobalFloat(string name, float value)
        {
            OutputFloats[name] = value;
        }

        public string GetGlobalString(string name)
        {
            if (OutputStrings.TryGetValue(name, out string value))
                return value;
            return string.Empty;
        }

        public void SetGlobalString(string name, string value)
        {
            OutputStrings[name] = value;
        }
    }
}
