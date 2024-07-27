using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace copy_flyouts.Resources
{
    public class HorizontalScreenAllignments
    {
        public static readonly Allignment Left = new("Left", 0.02);
        public static readonly Allignment Center = new("Center", 0.5);
        public static readonly Allignment Right = new("Right", 0.98);

        public static readonly List<Allignment> Allignments = new List<Allignment> { Left, Center, Right};

        public static Allignment? Find(string name)
        {
            foreach (Allignment allignment in Allignments)
            {
                if (name.Equals(allignment.ToString())) return allignment;
            }

            return null;
        }
    }
}
