using PathEarlScout;
using System;
using System.Collections.Generic;
using System.Text;

namespace UT_PathEarlScout
{
    public static class BasicInfoAccess
    {
        private static InfoAccess<BasicTileInfo> Access;
        private static void MakeInfoAccess()
        {
            Access = new InfoAccess<BasicTileInfo>();

            Access.GetStrings.Add("type", BasicTileInfo.GetTileType);
            Access.GetStrings.Add("archetype", BasicTileInfo.GetTileArchetype);
            Access.GetStrings.Add("height", BasicTileInfo.GetTileHeight);
            Access.GetStrings.Add("feature", BasicTileInfo.GetTileFeature);

            Access.SetStrings.Add("type", BasicTileInfo.SetTileType);
            Access.SetStrings.Add("height", BasicTileInfo.SetTileHeight);
            Access.SetStrings.Add("feature", BasicTileInfo.SetTileFeature);
        }

        public static InfoAccess<BasicTileInfo> GetInfoAccess()
        {
            if (Access == null)
                MakeInfoAccess();
            return Access;
        }
    }
}
