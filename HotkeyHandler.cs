using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace copy_flyouts
{
    /// <summary>
    /// Handles everything to do with the Ctrl+C hotkey.
    /// </summary>
    public class HotkeyHandler
    {
        private Window affectedWindow; // the window we register the hotkey on

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public bool IsRegistered { get; private set; } = false;

        private const int HOTKEY_ID = 9000;
        private HwndSource _source;

        private Flyout? currentFlyout = null;
        private DispatcherTimer? currentTimer = null;

        private ClipboardContent previousClipboard = new ClipboardContent(); // gets the last clipboard item on initialization

        public HotkeyHandler(Window affectedWindow)
        {
            this.affectedWindow = affectedWindow;

            // Register Ctrl + + C as global hotkey
            Register();
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
            // unregisters hotkey to send in a standard Ctrl+C to copy stuff
            UnregisterHotKey(new WindowInteropHelper(affectedWindow).Handle, HOTKEY_ID);

            SendKeys.SendWait("^c"); // sends the Ctrl+C command that will be handled normally now

            // registers hotkey again
            var helper = new WindowInteropHelper(affectedWindow);
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));

            // closes the existing flyout and stops the timer immediately
            CloseFlyout();

            System.Threading.Thread.Sleep(100); // wait a little bit to prevent clipboard access conflict
            // gets the text from the clipboard
            ClipboardContent clipboard = new ClipboardContent();
            bool copyIsEmpty = clipboard.Text.Length == 0;

            // creates and show the new flyout
            var flyout = new Flyout(clipboard);
            if (previousClipboard != null && (previousClipboard.Equals(clipboard)))
            {
                flyout.PlayErrorSound();
                flyout.SetToErrorIcon();
            }
            flyout.Show();

            previousClipboard = clipboard;

            // updates the current flyout reference
            currentFlyout = flyout;

            // creates a DispatcherTimer to close the flyout after 1.5 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                if (flyout == currentFlyout) // checks if the flyout is still the current one
                {
                    flyout.Close();
                    currentFlyout = null;
                }
            };
            timer.Start();

            // updates the current timer reference
            currentTimer = timer;
        }

        public void CloseFlyout()
        {
            if (currentFlyout != null)
            {
                currentFlyout.Close();
                currentFlyout = null;
            }
            if (currentTimer != null)
            {
                currentTimer.Stop();
                currentTimer = null;
            }
        }

        /// <summary>
        /// Registers the hotkey. This is public so that the user can invoke this via clicking buttons.
        /// </summary>
        public void Register()
        {
            var helper = new WindowInteropHelper(affectedWindow);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(WndProc);

            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));

            IsRegistered = true;
        }

        /// <summary>
        /// Get rid of the hotkey. This is public to ensure that this can be invoked on closing the MainWindow.
        /// </summary>
        public void Unregister()
        {
            CloseFlyout();
            _source.RemoveHook(WndProc);
            UnregisterHotKey(new WindowInteropHelper(affectedWindow).Handle, HOTKEY_ID);

            IsRegistered = false;
        }

        ~HotkeyHandler()
        {
            Unregister();
        }
    }
}
