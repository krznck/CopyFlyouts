using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace copy_flyouts.Resources
{
    public class DummyImage
    {
        public Image Image { get; private set; }

        public DummyImage(Uri imageUri) 
        {
            if (imageUri == null)
            {
                throw new ArgumentNullException(nameof(imageUri));
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(new BitmapImage(imageUri)));

                encoder.Save(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                Image = System.Drawing.Image.FromStream(memoryStream);
            }
        }
    }
}
