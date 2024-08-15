namespace CopyFlyouts.Resources
{
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Simple class that takes on an asset Uri and turns it into a usable <see cref="System.Drawing.Image"/> object.
    /// Meant to be used by <see cref="DummyDataHolder"/>.
    /// </summary>
    public class DummyImage
    {
        public Image Image { get; private set; }

        public DummyImage(Uri imageUri)
        {
            ArgumentNullException.ThrowIfNull(imageUri);

            using MemoryStream memoryStream = new();
            JpegBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(new BitmapImage(imageUri)));

            encoder.Save(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            Image = Image.FromStream(memoryStream);
        }
    }
}
