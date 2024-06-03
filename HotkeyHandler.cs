using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace copy_flash_wpf
{
    /// <summary>
    /// Handles everything to do with the Ctrl+C hotkey.
    /// </summary>
    class HotkeyHandler
    {
        private Window affectedWindow; // the window we register the hotkey on

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private HwndSource _source;

        public HotkeyHandler(Window affectedWindow)
        {
            this.affectedWindow = affectedWindow;
            var helper = new WindowInteropHelper(affectedWindow);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(WndProc);

            // Register Ctrl + + C as global hotkey
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID) // if the message matches the hotkey
            {
                Debug.WriteLine("Hotkey pressed");
                handled = true;
                HandleHotkey();
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Determines what happens when a hotkey is pressed.
        /// </summary>
        private void HandleHotkey()
        {
            // unregister hotkey to send in a standard Ctrl+C to copy stuff
            UnregisterHotKey(new WindowInteropHelper(affectedWindow).Handle, HOTKEY_ID);

            SendKeys.SendWait("^c"); // send the Ctrl+C command that will be handled normally now

            // register hotkey again
            var helper = new WindowInteropHelper(affectedWindow);
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));

            string clipboard = System.Windows.Clipboard.GetText();
            var flyout = new Flyout(clipboard.Trim());
            flyout.Show();

            // Create a DispatcherTimer to close the flyout after 1.5 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                flyout.Close();
            };
            timer.Start();
        }

        /// <summary>
        /// Get rid of the hotkey. This is public to ensure that this can be invoked on closing the MainWindow.
        /// </summary>
        public void Unregister()
        {
            _source.RemoveHook(WndProc);
            UnregisterHotKey(new WindowInteropHelper(affectedWindow).Handle, HOTKEY_ID);
        }

        ~HotkeyHandler()
        {
            Unregister();
        }
    }
}
