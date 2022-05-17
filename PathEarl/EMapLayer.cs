using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore
{
    [Flags]
    public enum EMapLayer
    {
        None = 0,
        Ground = 1,
        GroundSupport = 2,
        Sea = 4,
        SeaSupport = 8,
        Air = 16,
        AirSupport = 32,
    }
}
