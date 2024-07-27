using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class Allignment
    {
        public readonly string Name;
        public readonly double Value;

        public Allignment(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
