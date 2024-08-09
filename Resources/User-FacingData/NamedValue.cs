namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Simple flexible class that represents a double with a name.
    /// The purpose of this class is to create objects that can be visualized within user-facing UI,
    /// while containing information processable behind the scenes.
    /// </summary>
    /// <remarks>
    /// Similar in nature to <see cref="NamedAssetPath"/>
    /// </remarks>
    /// <param name="name">Name of the value.</param>
    /// <param name="value">Value to be processed.</param>
    public class NamedValue(string name, double value)
    {
        public readonly string Name = name;
        public readonly double Value = value;

        public override string ToString()
        {
            return Name;
        }
    }
}
