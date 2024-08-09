namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Static class representing hardcoded allignments for where the <see cref="Flyout"/>
    /// should be shown on the screen's vertical axis.
    /// </summary>
    public static class VerticalScreenAllignments
    {
        public static readonly NamedValue Top = new ("Top", 0.02);
        public static readonly NamedValue TopCenter = new ("Top-Center", 0.2);
        public static readonly NamedValue Center = new ("Center", 0.5);
        public static readonly NamedValue BottomCenter = new ("Bottom-Center", 0.8);
        public static readonly NamedValue Bottom = new ("Bottom", 0.98);

        public static readonly List<NamedValue> Allignments = [Top, TopCenter, Center, BottomCenter, Bottom];

        public static NamedValue? Find(string name)
        {
            foreach (NamedValue allignment in Allignments)
            {
                if (name.Equals(allignment.ToString())) return allignment;
            }

            return null;
        }
    }
}
