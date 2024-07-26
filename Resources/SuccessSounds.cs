using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class SuccessSounds
    {
        public static readonly Sound Osu = new Sound("Osu", "copy_flyouts.Assets.Audio.osu.wav");
        public static readonly Sound Beep = new Sound("Beep", "copy_flyouts.Assets.Audio.beep.wav");
        public static readonly List<Sound> Sounds = new List<Sound> { Osu, Beep };

        public static Sound? Find(string name)
        {
            foreach (Sound sound in Sounds)
            {
                if (sound.Name.Equals(name)) return sound;
            }

            return null;
        }
    }
}
