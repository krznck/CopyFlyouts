using copy_flyouts;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Color = System.Windows.Media.Color;

namespace copy_flyouts
{
    public partial class Flyout : Window
    {
        // magic attributes that make MakeToolWindow work :)
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_TRANSPARENT = 0x00000020;

        public Flyout(ClipboardContent clipContent)
        {
            InitializeComponent();
            this.Loaded += Flyout_Loaded;
            text.Text = clipContent.Text;

            if (clipContent.fileAmount > 0)
            {
                AddFileIcon();
            }
            if (clipContent.fileAmount > 1)
            {
                amount.Value = clipContent.fileAmount.ToString();
                amount.Visibility = Visibility.Visible;
            }

            bool copyHasNoText = clipContent.Text.Length == 0;
            bool copyHasImage = clipContent.image != null;

            // note: we're checking for image here, because clipboard history will only retain an image without its path.
            // so without this check, it will show the image, but also that no text was copied, which isn't false, but misleading
            if (copyHasNoText && !copyHasImage)
            {
                SetToErrorIcon();
                PlayErrorSound();
                text.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dc626d"));
                text.Text = "Copied text is empty!";
            }

            if (copyHasImage)
            {
                AddImageIcon();
                flyoutImage.Source = ConvertDrawingImageToWPFImage(clipContent.image);
                flyoutImage.Visibility = Visibility.Visible;

                if (copyHasNoText)
                {
                    Grid.SetColumnSpan(fileGrid, 2);
                }
            }

            this.ShowInTaskbar = false;
            this.Focusable = false;
        }

        /// <summary>
        /// Makes the flyout appear in the center of the screen, and a little to the bottom.
        /// Utilizes the currently focused monitor - not always the main monitor.
        /// </summary>
        private void Flyout_Loaded(object sender, RoutedEventArgs e)
        {
            // get the current active screen where the cursor is located
            var currentScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);

            // use the working area of the current screen to place the flyout
            var workingArea = currentScreen.WorkingArea;
            var windowWidth = this.ActualWidth;
            var windowHeight = this.ActualHeight;

            // here, we use the display transformation matrix to determine the scaling factor of the system,
            // which allows the flyout to be displayed at the correct place regardless of system scaling
            Matrix? matrix = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice;
            if (matrix.HasValue)
            {
                double dpiXFactor = matrix.Value.M11; // horizontal scaling factor
                double dpiYFactor = matrix.Value.M22; // vertical scaling factor

                // center the flyout within the working area of the current screen (and a little bit to the bottom)
                this.Left = workingArea.Left + (workingArea.Width - windowWidth * dpiXFactor) / 2 / dpiXFactor;
                this.Top = workingArea.Top + (workingArea.Height - windowHeight * dpiYFactor) / 1.2 / dpiYFactor;
            }

        }

        public void PlayErrorSound()
        {
            SoundPlayer player = new SoundPlayer(@"assets\audio\damage.wav");
            player.Load();
            player.Play();
        }

        /// <summary>
        /// Overloads/overrides the standard Show() method to also make it a tool window on each show.
        /// </summary>
        public new void Show()
        {
            base.Show();
            MakeToolWindowAndClickThrough(this);
        }

        /// <summary>
        /// Makes a window (this window) a tool window and click-through, therefore not displaying it in alt-tab and allowing clicks to pass through.
        /// </summary>
        private static void MakeToolWindowAndClickThrough(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT; // Combine styles with bitwise OR
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle);
        }

        public void SetToErrorIcon()
        {
            icon.Symbol = SymbolRegular.ClipboardError16;
            icon.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#dc626d"));
        }

        private void AddSecondIcon(SymbolRegular symbol)
        {
            icon2.Symbol = symbol;
            icon2.Visibility = Visibility.Visible;
            icon.Margin = new Thickness(0, 0, 10, 0);

            fileGrid.Visibility = Visibility.Visible;
        }

        private void AddFileIcon()
        {
            AddSecondIcon(SymbolRegular.DocumentCopy16);
            icon2.Width = 33;
        }

        private void AddImageIcon()
        {
            AddSecondIcon(SymbolRegular.ImageCopy24);
            icon2.Width = 35;
        }

        /// <summary>
        /// WPF-UI's ImageIcon does not accept System.Drawing.Image object, so this converts them to BitmapImage objects.
        /// </summary>
        private ImageSource ConvertDrawingImageToWPFImage(System.Drawing.Image drawingImage)
        {
            using (var ms = new MemoryStream())
            {
                // save to memory stream
                drawingImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                // then create a new BitmapImage and set its properties
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}