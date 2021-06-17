using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSweeper
{
    public class Save
    {
        public int[,] Field { get; set; }
        public FieldTag[,] ShadowOfWar { get; set; }

        public Cursor Cursor { get; set; }

        public Stats Stats { get; set; }

        public Cheats Cheats { get; set; }

    }
}
