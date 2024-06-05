using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

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

    public Flyout(string textToShow)
    {
        InitializeComponent();
        this.Loaded += Flyout_Loaded;
        text.Text = textToShow;

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

        // center the flyout within the working area of the current screen (and a little bit to the bottom)
        this.Left = workingArea.Left + (workingArea.Width - windowWidth) / 2;
        this.Top = workingArea.Top + (workingArea.Height - windowHeight) / 1.2;
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
        BitmapImage bitmapImage = new BitmapImage(new Uri("pack://application:,,,/assets/icons/ic_fluent_clipboard_error_16_filled.png", UriKind.RelativeOrAbsolute));
        icon.Source = bitmapImage;
    }
}
