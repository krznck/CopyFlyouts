using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace copy_flyouts.Resources
{
    public class DummyDataHolder
    {
        private static DummyImage tina1 = new DummyImage(new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina1.jpg"));
        private static DummyImage tina2 = new DummyImage(new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina2.jpg"));
        private static DummyImage tina3 = new DummyImage(new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina3.jpg"));
        private static DummyImage tina4 = new DummyImage(new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina4.jpg"));
        private static DummyImage tina5 = new DummyImage(new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina5.jpg"));

        private List<DummyImage> dummyImages = new List<DummyImage> { tina1, tina2, tina3, tina4, tina5 };

        public System.Drawing.Image CurrentImage { get; private set; } = tina1.Image;

        public void Refresh()
        {
            int currentIndex = dummyImages.FindIndex(image => image.Image == CurrentImage); // gets the index of dummyImages based on the CurrentImage

            if (currentIndex == -1)
            {
                CurrentImage = dummyImages[0].Image;
            }
            else
            {
                int nextIndex = (currentIndex + 1) % dummyImages.Count;
                CurrentImage = dummyImages[nextIndex].Image;
            }
        }
    }
}
