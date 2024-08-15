namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Simple flexible class that represents a path with a name.
    /// The purpose of this class is to create objects that can be visualized within user-facing UI,
    /// while containing paths processable behind the scenes that wouldn't be so elegant to show the user.
    /// </summary>
    /// <remarks>
    /// Similar in nature to <see cref="NamedValue"/>
    /// </remarks>
    /// <param name="name">Name of the asset.</param>
    /// <param name="assetPath">Path of an asset to be processed.</param>
    public class NamedAssetPath(string name, string assetPath)
    {
        public string Name { get; } = name;
        public string AssetPath { get; } = assetPath;

        public override string ToString()
        {
            return Name;
        }
    }
}
