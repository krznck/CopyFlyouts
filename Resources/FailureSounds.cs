using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class FailureSounds
    {
        public static readonly Sound Damage = new Sound("Damage", "copy_flyouts.assets.audio.damage.wav");
        public static readonly Sound Triangle = new Sound("Triangle", "copy_flyouts.Assets.Audio.triangle.wav"); // no idea why this has to be capitalized
        public static readonly Sound Square = new Sound("Square", "copy_flyouts.Assets.Audio.square.wav");
        public static readonly List<Sound> Sounds = new List<Sound> { Damage, Triangle, Square };

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
