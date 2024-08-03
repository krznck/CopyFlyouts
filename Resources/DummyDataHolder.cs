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

        private static string _explanation = "A little toolbox to try the flyouts in action with dummy data. Select text and use Ctrl+C to copy it, or use the copy button on the right. Copy dummy files or images with the buttons on the left. Refresh dummy data with the refresh button.";

        private List<DummyImage> dummyImages = new List<DummyImage> { tina1, tina2, tina3, tina4, tina5 };

        public System.Drawing.Image CurrentImage { get; private set; } = tina1.Image;
        public string CurrentText { get; private set; } = _explanation;

        public void Refresh()
        {
            RefreshImage();
            RefreshText();
        }

        private void RefreshImage()
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

        private void RefreshText()
        {
            string[] words = CurrentText.Split(' ');

            // shuffles the words around with Fisher-Yates shuffle
            Random rng = new Random();
            int n = words.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = words[k];
                words[k] = words[n];
                words[n] = value;
            }

            CurrentText = string.Join(" ", words);
        }

        public void ResetText()
        {
            CurrentText = _explanation;
        }
    }
}
