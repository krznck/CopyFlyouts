using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace copy_flyouts.Core
{
    public class ClipboardContent
    {
        private string text { get; set; } = "";
        public int fileAmount { get; set; } = 0;
        public Image? image { get; set; } = null;
        private Settings userSettings { get; set; }

        public ClipboardContent(Settings userSettings)
        {
            this.userSettings = userSettings;
            CheckSystemClipboard();

            // this while loop is here to *hopefully* prevent a bug where sometimes a copy will be successful,
            // but ClipBboardContent is unable to retrieve it.
            // That the program cannot access the system clipboard at the time is my guess to why this happens,
            // so a retry might fix that.
            int failure = 0;
            while (text.Equals("") && fileAmount == 0 && image is null && failure < 5)
            {
                Thread.Sleep(1);
                CheckSystemClipboard();
                failure++;
            }

        }

        private void CheckSystemClipboard()
        {
            if (Clipboard.ContainsText())
            {
                Text = Clipboard.GetText();
            }

            if (Clipboard.ContainsFileDropList())
            {
                string combinedPaths = "";
                foreach (string filePath in Clipboard.GetFileDropList())
                {
                    combinedPaths += filePath + " ; ";
                    fileAmount++;
                }
                Text = combinedPaths.TrimEnd(new char[] { ' ', ';' }); // removes the trailing semicolon
            }

            // this is done both here and in Flyout, so that we never call GetImage on the system clipboard if it's not needed
            // as that turns out to be heavy for the machine when the image is large enough
            bool hasImage = Clipboard.ContainsImage();
            if (hasImage && userSettings.AllowImages)
            {
                this.image = Clipboard.GetImage();
            }
            else if (hasImage && !userSettings.AllowImages)
            {
                this.image = new Bitmap(1, 1); // creates a fake image that can be passed on and easily detected
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                value = Regex.Replace(value, @"\s+", " "); // replaces all whitespace with one space
                text = value.Trim();
            }
        }

        public override bool Equals(object? obj)
        {
            var other = obj as ClipboardContent;
            if (other == null)
            { return false; }

            return text == other.text
                && fileAmount == other.fileAmount
                && (
                    image == null && other.image == null
                    || 
                        image != null && other.image != null
                        && ImageToByteArray(image).SequenceEqual(ImageToByteArray(other.image))
                        
                    );
        }

        public override int GetHashCode()
        {
            if (image == null)
            {
                return HashCode.Combine(text, fileAmount);
            }
            return HashCode.Combine(text, fileAmount, ImageToByteArray(image));
        }

        private byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
