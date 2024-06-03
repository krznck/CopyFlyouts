using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace copy_flash_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private HwndSource _source;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(WndProc);

            // Register Ctrl + + C as global hotkey
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _source.RemoveHook(WndProc);
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID)
            {
                Debug.WriteLine("Hotkey pressed");
                handled = true;
                // unregister hotkey to send in a standard Ctrl+C to copy stuff
                UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID);

                SendKeys.SendWait("^c"); // send the Ctrl+C command that will be handled normally now

                // register hotkey again
                var helper = new WindowInteropHelper(this);
                RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));

                string clipboard = System.Windows.Clipboard.GetText();
                copyText.Text = clipboard;
                var flyout = new Flyout(clipboard);
                flyout.Show();
            }
            return IntPtr.Zero;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            string clipboard = System.Windows.Clipboard.GetText();
            copyText.Text = clipboard;

            var flyout = new Flyout(clipboard);
            flyout.Show();
        }
    }
}