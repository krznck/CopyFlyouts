using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
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

    public Flyout(string textToShow)
    {
        InitializeComponent();
        this.Loaded += Flyout_Loaded;
        text.Text = textToShow;

        this.ShowInTaskbar = false;
        this.Focusable = false;
    }

    private void Flyout_Loaded(object sender, RoutedEventArgs e)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var windowWidth = this.ActualWidth;
        var windowHeight = this.ActualHeight;

        this.Left = (screenWidth / 2) - (windowWidth / 2);
        this.Top = (screenHeight / 1.2) - (windowHeight / 2);
    }

    /// <summary>
    /// Overloads/overrides the standard Show() method to also make it a tool window on each show.
    /// </summary>
    public new void Show()
    {
        base.Show();
        MakeToolWindow(this);
    }

    /// <summary>
    /// Makes a window (this window) a tool window, therefore not displaying it in alt-tab.
    /// </summary>
    public static void MakeToolWindow(Window window)
    {
        WindowInteropHelper helper = new WindowInteropHelper(window);
        int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
        exStyle |= WS_EX_TOOLWINDOW;
        SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle);
    }

    public void SetToErrorIcon()
    {
        BitmapImage bitmapImage = new BitmapImage(new Uri("pack://application:,,,/assets/icons/ic_fluent_clipboard_error_16_filled.png", UriKind.RelativeOrAbsolute));
        icon.Source = bitmapImage;
    }
}
