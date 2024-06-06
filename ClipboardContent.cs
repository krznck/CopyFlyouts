﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace copy_flash_wpf
{
    public class ClipboardContent
    {
        private string text { get; set; } = "";
        public int fileAmount { get; } = 0;
        public Image? image { get; } = null;

        public ClipboardContent()
        {
            if (Clipboard.ContainsText())
            {
                Text = Clipboard.GetText();
            } else if (Clipboard.ContainsFileDropList())
            {
                string combinedPaths = "";
                foreach (string filePath in Clipboard.GetFileDropList())
                {
                    combinedPaths += filePath + " ; ";
                    fileAmount++;
                }
                Text = combinedPaths.TrimEnd(new char[] { ' ', ';' }); // removes the trailing semicolon
            }
            
            if (Clipboard.ContainsImage())
            {
                image = Clipboard.GetImage();
            }
        }

        public string Text
        {
            get { return text;  }
            set
            {
                value = Regex.Replace(value, @"\s+", " "); // replaces all whitespace with one space
                this.text = value.Trim();
            }
        }

        public override bool Equals(object? obj)
        {
            var other = obj as ClipboardContent;
            if (other == null)
                { return false; }

            return (this.text == other.text) 
                && (this.fileAmount == other.fileAmount) 
                && (
                    (this.image == null && other.image == null) 
                    || (
                        (this.image != null && other.image != null)
                        && (ImageToByteArray(this.image).SequenceEqual(ImageToByteArray(other.image)))
                        )
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

        private byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
