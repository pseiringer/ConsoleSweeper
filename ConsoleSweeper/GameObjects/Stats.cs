using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSweeper
{
    public class Stats
    {
        public Stats() : this(new TimeSpan()) { }
        public Stats(TimeSpan timeSpent)
        {
            TimeSpent = timeSpent;
            Flags = 0;
            Questions = 0;
            Opens = 0;
            Chords = 0;
            ChordFlags = 0;
            ThreeBV = -1;
        }


        public TimeSpan TimeSpent { get; set; }

        public int Flags { get; set; }
        public int Questions { get; set; }
        public int Opens { get; set; }
        public int Chords { get; set; }
        public int ChordFlags { get; set; }

        public int ThreeBV { get; set; }

        public int Clicks() => Flags + Questions + Opens + Chords + ChordFlags;

        public double ClicksPerSec() => ClicksPerSec(2);
        public double ClicksPerSec(int digits) => TimeSpent.TotalSeconds > 0 ? Math.Round(Clicks() / TimeSpent.TotalSeconds, digits) : -1d;

        public double ThreeBVPerSec() => ThreeBVPerSec(2);
        public double ThreeBVPerSec(int digits) => TimeSpent.TotalSeconds > 0 ? Math.Round(ThreeBV / TimeSpent.TotalSeconds, digits) : -1d;

        public double Efficiency() => Efficiency(0);
        public double Efficiency(int digits) => Clicks() > 0 ? Math.Round(((double)ThreeBV / Clicks()) * 100, digits) : -1;
    }
}
