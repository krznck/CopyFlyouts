namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Static class representing hardcoded allignments for where the <see cref="Flyout"/>
    /// should be shown on the screen's horizontal axis.
    /// </summary>
    public class HorizontalScreenAllignments
    {
        public static readonly NamedValue Left = new ("Left", 0.02);
        public static readonly NamedValue Center = new ("Center", 0.5);
        public static readonly NamedValue Right = new ("Right", 0.98);

        public static readonly List<NamedValue> Allignments = [Left, Center, Right];

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
