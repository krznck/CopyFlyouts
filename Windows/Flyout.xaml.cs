using copy_flyouts.Core;
using copy_flyouts.Resources;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Media.PlayTo;
using Wpf.Ui;
using Wpf.Ui.Appearance;
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

        private readonly Settings _userSettings;
        private readonly ClipboardContent _clipContent;
        private readonly ClipboardContent _previousClip;

        public Flyout(ClipboardContent previousClip, ClipboardContent clipContent, Settings userSettings)
        {
            InitializeComponent();
            _previousClip = previousClip;
            _clipContent = clipContent;
            _userSettings = userSettings;

            DataContext = _userSettings;
            text.Text = _clipContent.Text;

            if (_clipContent.fileAmount > 0)
            {
                AddFileIcon();
            }
            if (_clipContent.fileAmount > 1)
            {
                amount.Value = _clipContent.fileAmount.ToString();
                amount.Visibility = Visibility.Visible;
            }

            bool copyHasNoText = _clipContent.Text.Length == 0;
            bool copyHasImage = _clipContent.image != null;

            if (copyHasImage)
            {
                AddImageIcon();

                if (!(_clipContent.image.Height == 1 && _clipContent.image.Width == 1 && !_userSettings.AllowImages))
                {
                    flyoutImage.Source = ConvertDrawingImageToWPFImage(_clipContent.image);
                    flyoutImage.Visibility = Visibility.Visible;

                    if (copyHasNoText)
                    {
                        Grid.SetColumnSpan(fileGrid, 2);
                    }
                }
            }

            // note:
            // this line is crucial for having the flyouts appear at the right place,
            // despite it being bounded in the XAML. Unsure why
            MaxWidth = _userSettings.FlyoutWidth;
            MaxHeight = _userSettings.FlyoutHeight;

            this.Loaded += Flyout_Loaded;

            this.ShowInTaskbar = false;
            this.Focusable = false;

            if (_userSettings.InvertedTheme)
            {
                ApplyInverseTheme();
            }

            if (!_userSettings.EnableFlyoutAnimations)
            {
                FadeInAnimation.Duration = TimeSpan.FromSeconds(0);
                MoveUpAnimation.Duration = TimeSpan.FromSeconds(0);
            }
        }

        private void Flyout_Loaded(object sender, RoutedEventArgs e)
        {
            // subscribes to the SizeChanged event to ensure window size is correctly initialized
            this.SizeChanged += Flyout_SizeChanged;

            if (_userSettings.FlyoutUnderCursor)
            {
                PlaceWindowAtCursor();
            }
            else
            {
                PositionWindow();
            }
        }

        private void Flyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // unsubscribes after the first size change to avoid repeated repositioning
            this.SizeChanged -= Flyout_SizeChanged;


            if (_userSettings.FlyoutUnderCursor)
            {
                PlaceWindowAtCursor();
            }
            else
            {
                PositionWindow();
            }
        }

        /// <summary>
        /// Makes the flyout appear in the center of the screen, and a little to the bottom.
        /// Utilizes the currently focused monitor - not always the main monitor.
        /// </summary>
        private void PositionWindow()
        {
            // get all screens
            var screens = System.Windows.Forms.Screen.AllScreens;

            Screen? currentScreen;
            try // tries to get the chosen monitor number
            {
                int choice = int.Parse(_userSettings.FlyoutScreen.Substring(_userSettings.FlyoutScreen.Length - 1));
                int monitorIndex = choice - 1; // we substract 1 from it since the array of screen starts from 0 and not 1

                // if the monitorIndex is out of bounds, use the primary screen as default
                currentScreen = (monitorIndex >= 0 && monitorIndex < screens.Length)
                                    ? screens[monitorIndex]
                                    : System.Windows.Forms.Screen.PrimaryScreen;
            }
            catch (System.FormatException) // but if the choice was "Follow cursor" (or the user wrote something else into the settings)
            {
                currentScreen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position); // then the flyout follows the cursor instead
            }

            // uses the working area of the current screen to place the flyout
            var workingArea = currentScreen.WorkingArea;
            var windowWidth = this.ActualWidth;
            var windowHeight = this.ActualHeight;

            // here, we use the display transformation matrix to determine the scaling factor of the system,
            // which allows the flyout to be displayed at the correct place regardless of system scaling
            // (on laptops, very commonly 125% scaling is used instead of 100%)
            var matrix = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice;
            if (matrix.HasValue)
            {
                double dpiXFactor = matrix.Value.M11; // horizontal scaling factor
                double dpiYFactor = matrix.Value.M22; // vertical scaling factor

                // converts the working area to device-independent units
                double workingAreaWidthDiu = workingArea.Width / dpiXFactor;
                double workingAreaHeightDiu = workingArea.Height / dpiYFactor;

                double horizontalAllignment = HorizontalScreenAllignments.Find(_userSettings.FlyoutHorizontalAllignment).Value;
                double verticalAllignment = VerticalScreenAllignments.Find(_userSettings.FlyoutVerticalAllignment).Value;
                // centers the flyout within the working area of the current screen (and a little bit to the bottom)
                this.Left = workingArea.Left / dpiXFactor + (workingAreaWidthDiu - windowWidth) * horizontalAllignment; 
                this.Top = workingArea.Top / dpiYFactor + (workingAreaHeightDiu - windowHeight) * verticalAllignment;
            }
        }

        private void PlaceWindowAtCursor()
        {
            System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;

            PresentationSource source = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);
            if (source != null)
            {
                double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

                double dipX = cursorPosition.X * (96.0 / dpiX);
                double dipY = cursorPosition.Y * (96.0 / dpiY);

                this.Left = dipX;
                this.Top = dipY + 20;
            }
        }

        public void PlaySound(Sound sound)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = sound.ResourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();
                    player.Play();
                }
            }
        }

        /// <summary>
        /// Overloads/overrides the standard Show() method to also make it a tool window on each show.
        /// </summary>
        public new void Show()
        {
            bool copyHasNoText = _clipContent.Text.Length == 0;
            bool copyHasImage = _clipContent.image != null;

            // note: we're checking for image too, because clipboard history will only retain an image without its path.
            // so without this check, it will show the image, but also that no text was copied, which isn't false, but misleading
            if (copyHasNoText && !copyHasImage)
            {
                SetToErrorIcon();
                if (_userSettings.EnableErrorSound)
                {
                    PlaySound(FailureSounds.Find(_userSettings.ChosenErrorSound));
                }
                text.Foreground = (SolidColorBrush)System.Windows.Application.Current.Resources["colorStatusDangerForeground1"];
                text.Text = "Copied text is empty!";
            }
            // note: here, we're not setting this to an error if the copy is an image with no path but images are not allowed,
            // since comparing a ClipboardContent that just says "Clipboard only has an image, but I'm not telling you what it is!" can't reasonably be compared
            // to another instance of such a ClipboardContent. This is the downside of avodiing calling Clipboard.GetImage() for performance. User should be told this in an info tooltip.
            else if (_previousClip != null && _previousClip.Equals(_clipContent) && !(copyHasImage && copyHasNoText && !_userSettings.AllowImages))
            {
                SetToErrorIcon();
                if (_userSettings.EnableErrorSound)
                {
                    PlaySound(FailureSounds.Find(_userSettings.ChosenErrorSound));
                }
            }
            else if (_userSettings.EnableSuccessSound)
            {
                if (_userSettings.EnableSuccessSound)
                {
                    PlaySound(SuccessSounds.Find(_userSettings.ChosenSuccessSound));
                }
            }

            base.Show();
            MakeToolWindowAndClickThrough(); // this has to be after the base.Show() to work
        }

        /// <summary>
        /// Makes a window (this window) a tool window and click-through, therefore not displaying it in alt-tab and allowing clicks to pass through.
        /// </summary>
        private void MakeToolWindowAndClickThrough()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT;
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle);
        }

        private void SetToErrorIcon()
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
        }

        private void AddImageIcon()
        {
            AddSecondIcon(SymbolRegular.ImageCopy24);
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

        private void ApplyInverseTheme()
        {
            if (ApplicationThemeManager.GetAppTheme().Equals(ApplicationTheme.Dark))
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Resources/Light.xaml", UriKind.Relative) });
            }
            else
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Resources/Dark.xaml", UriKind.Relative) });
            }
        }
    }
}