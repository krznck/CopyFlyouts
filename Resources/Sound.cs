using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class Sound
    {
        public string Name { get; }
        public string ResourcePath { get; }

        public Sound(string name, string resourcePath)
        {
            Name = name; ResourcePath = resourcePath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
