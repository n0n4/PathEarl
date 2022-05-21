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

        public Map<T> Map;

        public InfoAccess()
        {
            // add default accessors
            GetInts.Add("id", (tile) => { return tile.Id; });
            GetInts.Add("blocking", (tile) => { return (int)tile.Blocks; });
            SetInts.Add("blocking", SetBlocking);
            SetInts.Add("block", AddBlock);
            SetInts.Add("unblock", Unblock);

            GetFloats.Add("x", (tile) => { return tile.X; });
            GetFloats.Add("y", (tile) => { return tile.Y; });
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
    }
}
