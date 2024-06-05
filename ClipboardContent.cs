﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace copy_flash_wpf
{
    public class ClipboardContent
    {
        private string text { get; set; } = "";
        public int fileAmount { get; } = 0;

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

            return (this.text == other.text) && (this.fileAmount == other.fileAmount);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(text, fileAmount);
        }
    }
}