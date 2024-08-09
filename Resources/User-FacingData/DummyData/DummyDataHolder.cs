namespace CopyFlyouts.Resources
{
    using System.Collections.Specialized;

    /// <summary>
    /// A holder of dummy data to show to the user to test the functionality of the flyouts while configuring them.
    /// Used by the <see cref="MainWindow"/>'s toolbox.
    /// Always holds one piece of specific data (image, string, file) that can be made different on refresh.
    /// </summary>
    public class DummyDataHolder
    {
        #region Hardcoded Values
        private static readonly DummyImage tina1 = new (new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina1.jpg"));
        private static readonly DummyImage tina2 = new (new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina2.jpg"));
        private static readonly DummyImage tina3 = new (new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina3.jpg"));
        private static readonly DummyImage tina4 = new (new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina4.jpg"));
        private static readonly DummyImage tina5 = new (new Uri("pack://application:,,,/Assets/DummyFiles/Images/tina5.jpg"));

        private static readonly string _explanation = "A little toolbox to try the flyouts in action with dummy data. Select text and use Ctrl+C to copy it, or use the copy button on the right. Copy dummy files or images with the buttons on the left. Refresh dummy data with the refresh button.";

        private static readonly StringCollection paths1 = ["C:\\these\\are\\fake\\files\\than\\cant\\be\\pasted.txt"];
        private static readonly StringCollection paths2 = ["C:\\absolute\\paths\\will\\be\\displayed\\when\\copying\\files.txt", "D:\\multiple\\files\\will\\show\\multiple\\paths.exe"];
        private static readonly StringCollection paths3 = [
                @"C:\Users\JohnDoe\Documents\Report.docx",
                @"C:\Users\JohnDoe\Pictures\Vacation\Beach.png",
                @"C:\Users\JohnDoe\Music\FavoriteSong.mp3",
                @"C:\Users\JohnDoe\Downloads\SoftwareInstaller.exe",
                @"C:\Users\JohnDoe\Desktop\Project\Presentation.pptx",
                @"C:\Users\JaneDoe\Work\Spreadsheet.xlsx",];
        #endregion

        private readonly List<DummyImage> _dummyImages = [tina1, tina2, tina3, tina4, tina5];
        private readonly List<StringCollection> _dummyPaths = [paths1, paths2, paths3];

        public Image CurrentImage { get; private set; } = tina1.Image;
        public string CurrentText { get; private set; } = _explanation;
        public StringCollection CurrentFiles { get; private set; } = paths1;

        /// <summary>
        /// Assigns new values to each dummy data that the user can copy.
        /// </summary>
        public void Refresh()
        {
            RefreshImage();
            RefreshText();
            RefreshFiles();
        }

        /// <summary>
        /// Reset's the dummy text back to the original instructions on how to use the toolbox.
        /// </summary>
        public void ResetText()
        {
            CurrentText = _explanation;
        }

        private void RefreshImage()
        {
            // gets the index of dummyImages based on the CurrentImage
            int currentIndex = _dummyImages.FindIndex(image => image.Image == CurrentImage);

            if (currentIndex == -1)
            {
                CurrentImage = _dummyImages[0].Image;
            }
            else
            {
                int nextIndex = (currentIndex + 1) % _dummyImages.Count;
                CurrentImage = _dummyImages[nextIndex].Image;
            }
        }

        private void RefreshText()
        {
            string[] words = CurrentText.Split(' ');

            // shuffles the words around with Fisher-Yates shuffle
            Random rng = new ();
            int n = words.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (words[n], words[k]) = (words[k], words[n]);
            }

            CurrentText = string.Join(" ", words);
        }

        private void RefreshFiles()
        {
            int currentIndex = _dummyPaths.FindIndex(sc => sc == CurrentFiles);

            if (currentIndex == -1)
            {
                CurrentFiles = _dummyPaths[0];
            }
            else
            {
                int nextIndex = (currentIndex + 1) % _dummyPaths.Count;
                CurrentFiles = _dummyPaths[nextIndex];
            }
        }
    }
}
