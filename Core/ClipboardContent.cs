namespace CopyFlyouts.Core
{
    using System.IO;
    using System.Text.RegularExpressions;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Represents items copied into the clipboard to be shown by a <see cref="Flyout"/>.
    /// I.e., prepares the contents of a clipboard for display.
    /// </summary>
    public partial class ClipboardContent
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceToSpaceRegex();
        private readonly BehaviorSettings _userBehaviorSettings;
        private string _copyText = "";

        public int FileAmount { get; set; } = 0;
        public Image? Image { get; set; } = null;

        /// <summary>
        /// The text copied into the clipboard.
        /// All instances of whitespace are cleaned into just one space,
        /// to not unnecessarily take up <see cref="Flyout"/> space.
        /// </summary>
        public string Text
        {
            get { return _copyText; }
            set
            {
                value = WhitespaceToSpaceRegex().Replace(value, " ");
                _copyText = value.Trim();
            }
        }

        /// <summary>
        /// Initializes the <see cref="ClipboardContent"/> instance.
        /// On this initialization, the object builds itself using the system <see cref="Clipboard"/>.
        /// </summary>
        /// <param name="behaviorSettings">
        /// User's <see cref="BehaviorSettings"/> object - 
        /// used here to determine whether we need to perform the potentially performance intenstive process of getting an image.
        /// </param>
        public ClipboardContent(BehaviorSettings behaviorSettings)
        {
            _userBehaviorSettings = behaviorSettings;
            LoadContent();
        }

        /// <summary>
        /// Wrapper for <see cref="GetDataFromClipboard"/>, 
        /// that calls it an additional five times if the initial call resulted in no data.
        /// </summary>
        /// <remarks>
        /// This is done due to the method sometimes failing to assign <see cref="Clipboard"/> data correctly,
        /// due to what I assume is a result of the quirkiness of the system clipboard being a precious shared resource.
        /// </remarks>
        private void LoadContent()
        {
            GetDataFromClipboard();

            int failure = 0;
            while (_copyText.Length.Equals(0) && FileAmount == 0 && Image is null && failure < 5)
            {
                Thread.Sleep(1);
                GetDataFromClipboard();
                failure++;
            }
        }

        /// <summary>
        /// Assigns <see cref="Clipboard"/> data to the object instance's attributes,
        /// after having processed them.
        /// </summary>
        private void GetDataFromClipboard()
        {
            if (Clipboard.ContainsText()) { Text = Clipboard.GetText(); }

            if (Clipboard.ContainsFileDropList())
            {
                string combinedPaths = "";
                foreach (string? filePath in Clipboard.GetFileDropList())
                {
                    combinedPaths += filePath + " ; ";
                    FileAmount++;
                }
                Text = combinedPaths.TrimEnd([' ', ';']); // removes the trailing semicolon
            }

            if (Clipboard.ContainsImage())
            {
                // we're using user settings here instead of simply not displaying the image in the Flyout,
                // so that we don't have to call GetImage(), which can be intensive on big images
                if (_userBehaviorSettings.AllowImages) { Image = Clipboard.GetImage(); }
                else { Image = new Bitmap(1, 1); } // creates a fake image that can be passed on and easily detected
            }
        }

        /// <summary>
        /// Equality check needs to be overriden to facilitate comparing old copy attemps with new ones.
        /// </summary>
        /// <remarks>
        /// It is, as far as I know, impossible to compare images from the <see cref="Clipboard"/> without
        /// *getting* those images. Hence, that functionality when displaying images if turned off.
        /// </remarks>
        /// <param name="obj">Object to be compared against. False if not <see cref="ClipboardContent"/> instance.</param>
        /// <returns>Booelan representing whether the two objects represent the same Clipboard values.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not ClipboardContent other) { return false; }

            return _copyText == other._copyText
                && FileAmount == other.FileAmount
                && (Image is null && other.Image is null
                    || (Image is not null && other.Image is not null
                        && 
                        ImageToByteArray(Image).SequenceEqual(ImageToByteArray(other.Image))));
        }

        public override int GetHashCode()
        {
            if (Image is null)
            {
                return HashCode.Combine(_copyText, FileAmount);
            }
            return HashCode.Combine(_copyText, FileAmount, ImageToByteArray(Image));
        }

        /// <summary>
        /// A way of seeing an image by its contents, rather than any other attribute.
        /// Used comparing and hashing our images.
        /// </summary>
        /// <param name="inputImage">Image to be converted.</param>
        /// <returns>Byte array representation of the input image.</returns>
        private static byte[] ImageToByteArray(Image inputImage)
        {
            using var ms = new MemoryStream();
            inputImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
