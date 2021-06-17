using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSweeper
{
    public class Util
    {
        #region unicode constants
        public static readonly string LineH = char.ConvertFromUtf32(0x2550);
        public static readonly string LineHSingle = char.ConvertFromUtf32(0x2500);
        public static readonly string LineV = char.ConvertFromUtf32(0x2551);

        public static readonly string CornerTL = char.ConvertFromUtf32(0x2554);
        public static readonly string CornerTR = char.ConvertFromUtf32(0x2557);
        public static readonly string CornerBL = char.ConvertFromUtf32(0x255A);
        public static readonly string CornerBR = char.ConvertFromUtf32(0x255D);

        public static readonly string SplitHB = char.ConvertFromUtf32(0x2566);
        public static readonly string SplitHT = char.ConvertFromUtf32(0x2569);
        public static readonly string SplitVL = char.ConvertFromUtf32(0x2563);
        public static readonly string SplitVR = char.ConvertFromUtf32(0x2560);
        public static readonly string SplitVLSingle = char.ConvertFromUtf32(0x2562);
        public static readonly string SplitVRSingle = char.ConvertFromUtf32(0x255F);
        public static readonly string Split4Way = char.ConvertFromUtf32(0x256C);

        public static readonly string SquareFilled = char.ConvertFromUtf32(0x2588);
        public static readonly string SquareCheckered = char.ConvertFromUtf32(0x2591);

        public static readonly string QuestionMark = char.ConvertFromUtf32(0x003F);
        public static readonly string Flag = char.ConvertFromUtf32(0x01F7);
        public static readonly string Mine = char.ConvertFromUtf32(0x263C);



        public static readonly string ArrowUp = char.ConvertFromUtf32(0x2191);
        public static readonly string ArrowLeft = char.ConvertFromUtf32(0x2190);
        public static readonly string ArrowDown = char.ConvertFromUtf32(0x2193);
        public static readonly string ArrowRight = char.ConvertFromUtf32(0x2192);


        public static string GetStringNTimes(string s, int n)
            => string.Concat(Enumerable.Repeat(s, n));
        #endregion

    }
}
