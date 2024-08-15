namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Static class representing hardcoded sounds that can play when a copy has failed,
    /// i.e. is a duplicate or does not contain anything.
    /// </summary>
    public static class FailureSounds
    {
        public static readonly NamedAssetPath Damage =
            new("Damage", "copy_flyouts.assets.audio.damage.wav");
        public static readonly NamedAssetPath Triangle =
            new("Triangle", "copy_flyouts.Assets.Audio.triangle.wav"); // no idea why this has to be capitalized
        public static readonly NamedAssetPath Square =
            new("Square", "copy_flyouts.Assets.Audio.square.wav");
        public static readonly List<NamedAssetPath> Sounds = [Damage, Triangle, Square];

        public static NamedAssetPath? Find(string name)
        {
            foreach (NamedAssetPath sound in Sounds)
            {
                if (sound.Name.Equals(name))
                    return sound;
            }

            return null;
        }
    }
}
