namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Static class representing hardcoded options for how often to check for updates.
    /// </summary>
    public static class UpdateFrequencies
    {
        public static readonly NamedValue FifteenMinutes = new("15 minutes", 15);
        public static readonly NamedValue ThirtyMinutes = new("30 minutes", 30);
        public static readonly NamedValue OneHour = new("1 hour", 60);
        public static readonly NamedValue TwoHours = new("2 hours", 120);
        public static readonly NamedValue FourHours = new("4 hours", 240);
        public static readonly NamedValue EightHours = new("8 hours", 480);
        public static readonly NamedValue SixteenHours = new("16 hours", 960);
        public static readonly NamedValue OneDay = new("1 day", 1440);

        public static readonly List<NamedValue> Frequencies =
        [
            FifteenMinutes,
            ThirtyMinutes,
            OneHour,
            TwoHours,
            FourHours,
            EightHours,
            SixteenHours,
            OneDay
        ];

        public static NamedValue? Find(string name)
        {
            foreach (NamedValue frequency in Frequencies)
            {
                if (name.Equals(frequency.ToString()))
                    return frequency;
            }

            return null;
        }
    }
}
