namespace CopyFlyouts.Resources
{
    /// <summary>
    /// Static class representing hardcoded sounds that can play when a copy is successful, i.e. new.
    /// </summary>
    public static class SuccessSounds
    {
        public static readonly NamedAssetPath Osu = new ("Osu", "copy_flyouts.Assets.Audio.osu.wav");
        public static readonly NamedAssetPath Beep = new ("Beep", "copy_flyouts.Assets.Audio.beep.wav");
        public static readonly NamedAssetPath Pip = new ("Pip", "copy_flyouts.Assets.Audio.pip.wav");
        public static readonly List<NamedAssetPath> Sounds = [Osu, Beep, Pip];

        public static NamedAssetPath? Find(string name)
        {
            foreach (NamedAssetPath sound in Sounds)
            {
                if (sound.Name.Equals(name)) return sound;
            }

            return null;
        }
    }
}
