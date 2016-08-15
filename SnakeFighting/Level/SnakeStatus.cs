using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeFighting.Level
{
    [Flags]
    public enum SnakeStatus : int
    {
        Normal = 0x0,
        Stunned = 0x1,
        AfterStunned = 0x2,
        Super = 0x4
    }
}
