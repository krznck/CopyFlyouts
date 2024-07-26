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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WK.Libraries.SharpClipboardNS;

namespace copy_flyouts.Core
{
    /// <summary>
    /// Handles everything to do with the Ctrl+C hotkey.
    /// </summary>
    public class HotkeyHandler
    {
        private Window affectedWindow; // the window we register the hotkey on
        private Settings userSettings;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private HwndSource _source;

        private Flyout? currentFlyout = null;
        private DispatcherTimer? currentTimer = null;

        private ClipboardContent previousClipboard; // gets the last clipboard item on initialization

        // will be used to monitor mouse-clicked copies and copies not started by the user
        private SharpClipboard sharpClipboard = new();

        private bool isInitialSubscription = true; // this ensures the above does not show the flyout of what's in the clipboard on opening the program

        public HotkeyHandler(Window affectedWindow, Settings userSettings)
        {
            this.affectedWindow = affectedWindow;
            this.userSettings = userSettings;
            previousClipboard = new ClipboardContent(userSettings);

            // subscribe to UserSettings changes
            userSettings.PropertyChanged += UserSettings_PropertyChanged;

            // ensures the mouse-click listener monitors all formats
            sharpClipboard.ObservableFormats.Texts = true;
            sharpClipboard.ObservableFormats.Files = true;
            sharpClipboard.ObservableFormats.Images = true;
            sharpClipboard.ObservableFormats.Others = false; // setting this to true creates unexpected behavior - see issue #37

            // register Ctrl + + C as global hotkey
            Register();
            // listens to non-keyboard copies
            if (userSettings.EnableNonKeyboardFlyouts && userSettings.FlyoutsEnabled) { sharpClipboard.ClipboardChanged += SharpClipboard_ClipboardChanged; }
        }

        private void SharpClipboard_ClipboardChanged(object? sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (isInitialSubscription)
            {
                isInitialSubscription = false;
                return;
            }

            ShowNewFlyout();

            // we remove and return the clipboard change listener on a short timer to prevent the bug of a copy being shown twice on the main window, or when screenshotting anything
            // this is ultimately a workaround, but should not significantly mess anything up (hopefully)
            sharpClipboard.ClipboardChanged -= SharpClipboard_ClipboardChanged;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Start();
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                sharpClipboard.ClipboardChanged += SharpClipboard_ClipboardChanged;
            };
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.FlyoutsEnabled))
            {
                if (userSettings.FlyoutsEnabled) { Register(); }
                else { Unregister(); }
            }

            if (e.PropertyName == nameof(Settings.EnableNonKeyboardFlyouts))
            {
                if (userSettings.EnableNonKeyboardFlyouts && userSettings.FlyoutsEnabled) { sharpClipboard.ClipboardChanged += SharpClipboard_ClipboardChanged; }
                else if (!userSettings.EnableKeyboardFlyouts || !userSettings.FlyoutsEnabled) { sharpClipboard.ClipboardChanged -= SharpClipboard_ClipboardChanged; }
            }
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID) // if the message matches the hotkey
            {
                Debug.WriteLine("Hotkey pressed");
                handled = true;
                sharpClipboard.ClipboardChanged -= SharpClipboard_ClipboardChanged; // we stop listening to other copies here, since we know this is a keyboard copy attempt

                // this if is here so that we handle the message and DON'T LISTEN TO KEYBOARD CHANGES,
                // but also not show the flyout if the user doesn't want it
                if (userSettings.EnableKeyboardFlyouts) { HandleHotkey(); }

                if (userSettings.EnableNonKeyboardFlyouts && userSettings.FlyoutsEnabled) { sharpClipboard.ClipboardChanged += SharpClipboard_ClipboardChanged; }
            }
            return nint.Zero;
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

            ShowNewFlyout();
        }

        private void ShowNewFlyout()
        {
            // closes the existing flyout and stops the timer immediately
            CloseFlyout();

            Thread.Sleep(100); // wait a little bit to prevent clipboard access conflict
            // gets the text from the clipboard
            ClipboardContent clipboard = new ClipboardContent(userSettings);
            bool copyIsEmpty = clipboard.Text.Length == 0;

            // creates and shows the new flyout
            var flyout = new Flyout(previousClipboard, clipboard, userSettings);

            flyout.Show();

            previousClipboard = clipboard;

            // updates the current flyout reference
            currentFlyout = flyout;

            // creates a DispatcherTimer to close the flyout after 1.5 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(userSettings.FlyoutLifetime) };
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
            if (userSettings.FlyoutsEnabled)
            {
                var helper = new WindowInteropHelper(affectedWindow);
                _source = HwndSource.FromHwnd(helper.Handle);
                _source.AddHook(WndProc);

                RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));
            }
        }

        /// <summary>
        /// Get rid of the hotkey. This is public to ensure that this can be invoked on closing the MainWindow.
        /// </summary>
        public void Unregister()
        {
            CloseFlyout();
            _source?.RemoveHook(WndProc);
            UnregisterHotKey(new WindowInteropHelper(affectedWindow).Handle, HOTKEY_ID);
        }

        ~HotkeyHandler()
        {
            Unregister();
        }
    }
}
