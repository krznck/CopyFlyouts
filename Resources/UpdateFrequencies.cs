using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class UpdateFrequencies
    {
        public static readonly Time FifteenMinutes = new("15 minutes", 15);
        public static readonly Time ThirtyMinutes = new("30 minutes", 30);
        public static readonly Time OneHour = new("1 hour", 60);
        public static readonly Time TwoHours = new("2 hours", 120);
        public static readonly Time FourHours = new("4 hours", 240);
        public static readonly Time EightHours = new("8 hours", 480);
        public static readonly Time SixteenHours = new("16 hours", 960);
        public static readonly Time OneDay = new("1 day", 1440);

        public static readonly List<Time> Frequencies = new List<Time> { FifteenMinutes, ThirtyMinutes, OneHour, TwoHours, FourHours, EightHours, SixteenHours, OneDay };

        public static Time? Find(string name)
        {
            foreach (Time frequency in Frequencies)
            {
                if (name.Equals(frequency.ToString())) return frequency;
            }

            return null;
        }
    }
}
