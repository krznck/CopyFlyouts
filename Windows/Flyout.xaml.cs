namespace CopyFlyouts
{
    using CopyFlyouts.Settings;
    using CopyFlyouts.Core;
    using CopyFlyouts.Resources;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Wpf.Ui.Appearance;
    using Wpf.Ui.Controls;

    /// <summary>
    /// Interaction logic for Flyout.xaml
    /// This represents the flyouts that show when a copy attempt is made.
    /// </summary>
    public partial class Flyout : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        private readonly SettingsManager _userSettings; // we use the entire settings, since Flyout uses both Appearance and Behavior
        private readonly ClipboardContent _clipContent;
        private readonly ClipboardContent _previousClip;

        // these methods are used to make it possible to click through the Flyout window instance
        // note: SYSLIB1054 suggests changing DllImport to LibraryImport, saying it may improve performance
        // however, flyout performance does not appear to be bad, and doing this requires enabling unsafe code.
        // I won't claim to know enough about this topic, so I'm leaving this as is.
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Initializes the <see cref="Flyout"/> instance, and sets many of the values related to the current clipboard content
        /// and how the flyout should appear, as well as the user settings configuration related to the flyout.
        /// </summary>
        /// <remarks>
        /// Note that not all things can be set here, and some must be done in <see cref="Show"/>.
        /// </remarks>
        /// <param name="previousClip">Previous clipboard content, to compare with current for success or failure.</param>
        /// <param name="clipContent">Current clipboard content, to be compared and shown.</param>
        /// <param name="userSettings">The settings necessary to manipulate the flyout per user's wishes.</param>
        public Flyout(ClipboardContent previousClip, ClipboardContent clipContent, SettingsManager userSettings)
        {
            InitializeComponent();
            _previousClip = previousClip;
            _clipContent = clipContent;
            _userSettings = userSettings;

            DataContext = _userSettings;

            text.Text = _clipContent.Text;

            if (_clipContent.FileAmount > 0) { AddFileIcon(); }

            if (_clipContent.FileAmount > 1)
            {
                amount.Value = _clipContent.FileAmount.ToString();
                amount.Visibility = Visibility.Visible;
            }

            if (_clipContent.Image is not null)
            {
                AddImageIcon();

                // the following check is to see that this is a flyout with an image that we are allowed to show
                // i.e., if the image is a dummy and the user has no need to see it, then there's no point in going further
                if (!(_clipContent.Image.Height == 1 
                    && _clipContent.Image.Width == 1 
                    && !_userSettings.Behavior.AllowImages))
                {
                    flyoutImage.Source = ConvertDrawingImageToWPFImage(_clipContent.Image);
                    flyoutImage.Visibility = Visibility.Visible;

                    if (_clipContent.Text.Length.Equals(0)) { Grid.SetColumnSpan(fileGrid, 2); }
                }
            }

            // note:
            // these two lines are crucial for having the flyouts appear at the right place,
            // despite it being bounded in the XAML. Unsure why
            MaxWidth = _userSettings.Appearance.FlyoutWidth;
            MaxHeight = _userSettings.Appearance.FlyoutHeight;

            Loaded += Flyout_Loaded;

            // we don't want this to look like a window
            ShowInTaskbar = false;
            Focusable = false;

            if (_userSettings.Appearance.InvertedTheme) { ApplyInverseTheme(); }

            if (!_userSettings.Behavior.EnableFlyoutAnimations)
            {
                FadeInAnimation.Duration = TimeSpan.FromSeconds(0);
                MoveUpAnimation.Duration = TimeSpan.FromSeconds(0);
            }
        }

        /// <summary>
        /// Responsible for positioning the window at the appropriate place (according to user seetings) after loading.
        /// </summary>
        /// <remarks>
        /// Note: Needs to be used with <see cref="Flyout_SizeChanged(object, SizeChangedEventArgs)"/> to work.
        /// </remarks>
        /// <param name="sender">Sender of the event - the Flyout instance itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void Flyout_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += Flyout_SizeChanged;

            if (_userSettings.Behavior.FlyoutUnderCursor) { PlaceWindowAtCursor(); }
            else { PositionWindow(); }
        }

        /// <summary>
        /// Responsible for positioning the window at the appropriate place (according to user settings) after one size change.
        /// <remarks>
        /// Note: Needs to be used with <see cref="Flyout_Loaded(object, RoutedEventArgs)"/> to work.
        /// </remarks>
        /// <param name="sender">Sender of the event - the Flyout instance itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void Flyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // unsubscribes after the first size change to avoid repeated repositioning
            SizeChanged -= Flyout_SizeChanged;

            if (_userSettings.Behavior.FlyoutUnderCursor) { PlaceWindowAtCursor(); }
            else { PositionWindow(); }
        }

        /// <summary>
        /// Positions the flyout in accordance with user settings.
        /// First determines which screen it should be displayed on, then which location on the screen.
        /// </summary>
        private void PositionWindow()
        {
            // gets all screens
            var screens = Screen.AllScreens;

            Screen? displayScreen;
            if (_userSettings.Behavior.FlyoutScreen.Contains("Follow cursor")) { displayScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position); }
            else
            {
                try // tries to get the chosen monitor number
                {
                    int choice = int.Parse(_userSettings.Behavior.FlyoutScreen[^1..]);
                    int monitorIndex = choice - 1; // we substract 1 from it since the array of screens starts from 0 and not 1

                    // if the monitorIndex is out of bounds, use the primary screen as default
                    displayScreen = (monitorIndex >= 0 && monitorIndex < screens.Length) ? screens[monitorIndex] : Screen.PrimaryScreen;
                }
                catch (FormatException) // but if the settings somehow had anything other than the allowed choice
                {
                    displayScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position); // then default to following cursor
                }

            }

            if (displayScreen is null) { throw new ArgumentNullException("Display screen not determined"); }

            // uses the working area of the screen to place the flyout
            var workingArea = displayScreen.WorkingArea;
            var windowWidth = ActualWidth;
            var windowHeight = ActualHeight;

            // here, we use the display transformation matrix to determine the scaling factor of the system,
            // which allows the flyout to be displayed at the correct place regardless of system scaling
            // (on laptops, very commonly 125% scaling is used instead of 100%)
            var matrix = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice;

            if (!matrix.HasValue) { throw new ArgumentException("Display transformation matrix has no value."); }

            double dpiXFactor = matrix.Value.M11; // horizontal scaling factor
            double dpiYFactor = matrix.Value.M22; // vertical scaling factor

            // converts the working area to device-independent units
            double workingAreaWidthDiu = workingArea.Width / dpiXFactor;
            double workingAreaHeightDiu = workingArea.Height / dpiYFactor;

            var horizontalAllignment = HorizontalScreenAllignments.Find(_userSettings.Behavior.FlyoutHorizontalAllignment);
            var verticalAllignment = VerticalScreenAllignments.Find(_userSettings.Behavior.FlyoutVerticalAllignment);

            if (horizontalAllignment is null || verticalAllignment is null) { throw new ArgumentNullException("Possible allignments not found."); }

            double horizontalValue = horizontalAllignment.Value;
            double verticalValue = verticalAllignment.Value;

            // finally puts the flyout at the appropriate place, in accordance with the settings
            Left = workingArea.Left / dpiXFactor + (workingAreaWidthDiu - windowWidth) * horizontalValue;
            Top = workingArea.Top / dpiYFactor + (workingAreaHeightDiu - windowHeight) * verticalValue;
        }

        /// <summary>
        /// Used instead of <see cref="PositionWindow"/>, when the user wants the flyouts under the cursor instead of fixed.
        /// </summary>
        private void PlaceWindowAtCursor()
        {
            PresentationSource source = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);
            if (source is null) { return; }

            double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

            System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
            double dipX = cursorPosition.X * (96.0 / dpiX);
            double dipY = cursorPosition.Y * (96.0 / dpiY);

            Left = dipX;
            Top = dipY + 20; // a little below the cursor
        }

        private static void PlaySound(NamedAssetPath sound)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = sound.AssetPath;

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                SoundPlayer player = new(stream);
                player.Load();
                player.Play();
            }
        }

        /// <summary>
        /// Shows the <see cref="Flyout"/> instance, after determining copy attempt failures and play the appropriate sounds at this stage.
        /// </summary>
        /// <remarks>
        /// <see cref="Window.Show()"/> cannot be overriden, hence method hiding was used instead.
        /// </remarks>
        public new void Show()
        {
            bool copyHasNoText = _clipContent.Text.Length.Equals(0);
            bool copyHasImage = _clipContent.Image is not null;

            // note: we're checking for image too, because clipboard history will only retain an image without its path.
            // so without this check, it will show the image, but also that no text was copied, which isn't false, but misleading
            if (copyHasNoText && !copyHasImage)
            {
                SetToErrorIcon();

                if (_userSettings.Behavior.EnableErrorSound)
                {
                    NamedAssetPath? sound = FailureSounds.Find(_userSettings.Behavior.ChosenErrorSound);
                    if (sound is not null) { PlaySound(sound); }
                }

                text.Foreground = (SolidColorBrush)System.Windows.Application.Current.Resources["colorStatusDangerForeground1"];
                text.Text = "Copied text is empty!";
            }

            // note: here, we're not setting this to an error if the copy is an image with no path but images are not allowed,
            // since comparing a ClipboardContent that just says "Clipboard only has an image, but I'm not telling you what it is!" can't reasonably be compared
            // to another instance of such a ClipboardContent. This is the downside of avoiding calling Clipboard.GetImage() for performance. User should be told this in an info tooltip.
            else if (_previousClip is not null && _previousClip.Equals(_clipContent) && !(copyHasImage && copyHasNoText && !_userSettings.Behavior.AllowImages))
            {
                SetToErrorIcon();

                if (_userSettings.Behavior.EnableErrorSound)
                {
                    NamedAssetPath? sound = FailureSounds.Find(_userSettings.Behavior.ChosenErrorSound);
                    if (sound is not null) { PlaySound(sound); }
                }
            }

            else if (_userSettings.Behavior.EnableSuccessSound)
            {
                if (_userSettings.Behavior.EnableSuccessSound)
                {
                    NamedAssetPath? sound = SuccessSounds.Find(_userSettings.Behavior.ChosenSuccessSound);
                    if (sound is not null) { PlaySound(sound); }
                }

            }
            base.Show();
            ConvertToToolWindowAndClickThrough(); // this has to be after the base.Show() to work
        }

        /// <summary>
        /// Makes a this flyout window a tool window, therefore not displaying it in alt-tab and allowing clicks to pass through.
        /// </summary>
        private void ConvertToToolWindowAndClickThrough()
        {
            WindowInteropHelper helper = new (this);
            int extendedWindowStyles = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            extendedWindowStyles |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT;
            _ = SetWindowLong(helper.Handle, GWL_EXSTYLE, extendedWindowStyles);
            // note: we are intentionally ignoring the return value, since we just want to apply the window styles
        }

        private void SetToErrorIcon()
        {
            icon.Symbol = SymbolRegular.ClipboardError16;
            icon.Foreground = (SolidColorBrush)System.Windows.Application.Current.Resources["colorStatusDangerForeground1"];
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
        /// <see cref="ImageIcon"/> does not accept <see cref="System.Drawing.Image"/> object, so this converts them to <see cref="BitmapImage"/> objects.
        /// </summary>
        private static BitmapImage ConvertDrawingImageToWPFImage(System.Drawing.Image drawingImage)
        {
            using var memoryStream = new MemoryStream();
            // save to memory stream
            drawingImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // then create a new BitmapImage and set its properties
            BitmapImage bitmapImage = new ();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void ApplyInverseTheme()
        {
            if (ApplicationThemeManager.GetAppTheme().Equals(ApplicationTheme.Dark))
            {
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Resources/Light.xaml", UriKind.Relative) });
            }
            else
            {
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Resources/Dark.xaml", UriKind.Relative) });
            }
        }
    }
}