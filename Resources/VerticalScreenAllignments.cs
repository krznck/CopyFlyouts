using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace copy_flyouts.Resources
{
    public class VerticalScreenAllignments
    {
        public static readonly Allignment Top = new("Top", 0.02);
        public static readonly Allignment TopCenter = new("Top-Center", 0.2);
        public static readonly Allignment Center = new("Center", 0.5);
        public static readonly Allignment BottomCenter = new("Bottom-Center", 0.8);
        public static readonly Allignment Bottom = new("Bottom", 0.98);

        public static readonly List<Allignment> Allignments = new List<Allignment> { Top, TopCenter, Center, BottomCenter, Bottom };

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
