namespace CopyFlyouts.Core
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using WK.Libraries.SharpClipboardNS;
    using CopyFlyouts.Settings;

    /// <summary>
    /// Responsible for catching and processing (i.e., generating flyouts for)
    /// keyboard (CTRL+C) and non-keyboard copy attempts.
    /// </summary>
    public class CopyCatcher
    {
        private readonly SettingsManager _userSettings;
        private Flyout? _currentFlyout = null;
        private DispatcherTimer? _currentTimer = null;
        private ClipboardContent _previousClipboard;
        private readonly SharpClipboard _sharpClipboard = new(); // will be used to monitor mouse-clicked copies and copies not started by the user
        private bool _isInitialSubscription = true; // this ensures the above does not show the flyout of what's in the clipboard on opening the program
        private bool _clipboardSubscribed = false;
        private HwndSource? _source;

        private const int HOTKEY_ID = 9000; // used for CTRL + C behavior

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        /// <summary>
        /// Initializes the <see cref="CopyCatcher"/> instance.
        /// Stores the state of the clipboard in memory to compare it to the next copy.
        /// Starts the CTRL+C and copy-listening behaviors.
        /// </summary>
        /// <param name="userSettings"></param>
        public CopyCatcher(SettingsManager userSettings)
        {
            _userSettings = userSettings;
            _previousClipboard = new ClipboardContent(_userSettings.Behavior);

            _userSettings.PropertyChanged += UserSettings_PropertyChanged;

            // ensures the mouse-click listener monitors all relevant formats
            _sharpClipboard.ObservableFormats.Texts = true;
            _sharpClipboard.ObservableFormats.Files = true;
            _sharpClipboard.ObservableFormats.Images = true;
            _sharpClipboard.ObservableFormats.Others = false; // setting this to true creates unexpected behavior

            // register Ctrl + C as global hotkey
            RegisterKeyboardCatching();
            // listens to non-keyboard copies
            if (_userSettings.Behavior.EnableNonKeyboardFlyouts && _userSettings.General.FlyoutsEnabled) { SubscribeToClipboard(); }
        }

        /// <summary>
        /// Event handler for the <see cref="SharpClipboard"/>'s ClipboardChanged event, 
        /// i.e. when the <see cref="Clipboard"/> has been modified.
        /// </summary>
        /// <param name="sender">Origin of the event - the <see cref="SharpClipboard"/> instance (unused).</param>
        /// <param name="e">Clipboard changed event arguments (unused).</param>
        private void SharpClipboard_ClipboardChanged(object? sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            // SharpClipboard detects a clipboard change when the program is turned on, I'm not sure why.
            // hence, it's ignored at first
            if (_isInitialSubscription)
            {
                _isInitialSubscription = false;
                return;
            }

            ShowNewFlyout();

            // we remove and return the clipboard change listener on a short timer to prevent the bug of a copy
            // sometimes being shown twice, like on the main window, or when screenshotting anything.
            // this is ultimately a workaround, but should not significantly mess anything up (hopefully)
            // note: could potentially be caused by the fact that we have to listen to multiple formats,
            // potentially triggering the event multiple times for one copy, when a copy has multiple DataFormats?
            UnscrubscribeFromClipboard();
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            timer.Start();
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                SubscribeToClipboard();
            };
        }

        /// <summary>
        /// Handler for <see cref="SettingsManager"/>'s PropertyChanged event.
        /// Responsible for enabling and disabling keyboard and non-keyboard copy catching
        /// when the relevant settings change.
        /// </summary>
        /// <param name="sender">Sender of the event - <see cref="SettingsManager"/> object (unused).</param>
        /// <param name="e">Property changed event arguments - used to see which property has changed.</param>
        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case (nameof(SettingsManager.General.FlyoutsEnabled)):

                    if (_userSettings.General.FlyoutsEnabled)
                    {
                        if (_userSettings.Behavior.EnableKeyboardFlyouts) { RegisterKeyboardCatching(); }
                        if (_userSettings.Behavior.EnableNonKeyboardFlyouts) { SubscribeToClipboard(); }
                    }
                    else
                    {
                        Unregister();
                        UnscrubscribeFromClipboard();
                    }

                    break;

                case (nameof(SettingsManager.Behavior.EnableNonKeyboardFlyouts)):

                    if (_userSettings.Behavior.EnableNonKeyboardFlyouts && _userSettings.General.FlyoutsEnabled) { SubscribeToClipboard(); }
                    else if (!_userSettings.Behavior.EnableKeyboardFlyouts || !_userSettings.General.FlyoutsEnabled) { UnscrubscribeFromClipboard(); }

                    break;

                default:
                    break;
            }
        }

        private void SubscribeToClipboard()
        {
            if (!_clipboardSubscribed) { 
                _sharpClipboard.ClipboardChanged += SharpClipboard_ClipboardChanged; 
                _clipboardSubscribed = true;
            }
        }

        private void UnscrubscribeFromClipboard()
        {
            if (_clipboardSubscribed) { 
                _sharpClipboard.ClipboardChanged -= SharpClipboard_ClipboardChanged; 
                _clipboardSubscribed = false;
            }
        }

        /// <summary>
        /// Windows Procedure method which triggers if the message received is that CTRL + C has been pressed,
        /// and subsequently kicks off the process of displaying a flyout for the keyboard copy attempt.
        /// </summary>
        /// <remarks>
        /// Since this is a keyboard copy, unsubscribes from listening to non-keyboard copies for the remainder
        /// of processing the message.
        /// </remarks>
        /// <param name="hwnd">Unique identifier of the window receiving the message (unused).</param>
        /// <param name="messageCode">Identifier for the type of message being sent - used to check whether CTRL + C was pressed.</param>
        /// <param name="wParam">Additional information about the message - in our case, further identifier for the hotkey.</param>
        /// <param name="lParam">Additional information about the message (unused).</param>
        /// <param name="handled">Flag to indicate that the message has been processed - true when we have a hit.</param>
        /// <returns>Indication that no specific result needs to be returned to the OS (nint.Zero).</returns>
        private nint WndProc(nint hwnd, int messageCode, nint wParam, nint lParam, ref bool handled)
        {
            if (messageCode == 0x0312 && wParam.ToInt32() == HOTKEY_ID) // if the message matches CTRL + C
            {
                Debug.WriteLine("Hotkey pressed");
                handled = true;
                UnscrubscribeFromClipboard(); // we stop listening to other copies here, since we know this is a keyboard copy attempt

                if (_userSettings.Behavior.EnableKeyboardFlyouts) { HandleKeyboardCopy(); }

                if (_userSettings.Behavior.EnableNonKeyboardFlyouts && _userSettings.General.FlyoutsEnabled) { SubscribeToClipboard(); }
            }
            return nint.Zero;
        }

        /// <summary>
        /// Responsible for what happens when CTRL + C has been pressed and the program received a message about it.
        /// Shows a new flyout as a result of the copy attempt.
        /// </summary>
        /// <remarks>
        /// Since it was us who consumed the CTRL + C, and not the process that was meant to receive it to trigger a copy,
        /// we temporarily remove the hotkey listening, and send in another CTRL + C to be processed normally - 
        /// but now with the knowledge that there's a potential copy waiting for us to process.
        /// </remarks>
        private void HandleKeyboardCopy()
        {
            // unregisters hotkey to send in a standard Ctrl+C to copy stuff
            UnregisterHotKey(new WindowInteropHelper(Application.Current.MainWindow).Handle, HOTKEY_ID);

            SendKeys.SendWait("^c"); // sends the Ctrl+C command that will be handled normally now

            // registers hotkey again
            var helper = new WindowInteropHelper(Application.Current.MainWindow);
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));

            ShowNewFlyout();
        }

        /// <summary>
        /// Creates a new <see cref="ClipboardContent"/> instance to get the contents of the clipboard,
        /// and then generates and shows a <see cref="Flyout"/> to show this content.
        /// </summary>
        /// <remarks>
        /// As it's possible that a flyout is already on screen when this is called, 
        /// the method starts with killing the former flyout.
        /// </remarks>
        private void ShowNewFlyout()
        {
            // closes the existing flyout and stops the timer immediately
            CloseFlyout();

            Thread.Sleep(100); // we wait a little bit to prevent clipboard access conflict

            ClipboardContent currentClipboard = new (_userSettings.Behavior);
            bool copyIsEmpty = currentClipboard.Text.Length == 0;

            var flyout = new Flyout(_previousClipboard, currentClipboard, _userSettings);
            flyout.Show();

            _previousClipboard = currentClipboard;
            _currentFlyout = flyout;

            // creates a DispatcherTimer to close the flyout after the user-specified time
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_userSettings.Behavior.FlyoutLifetime) };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                if (flyout == _currentFlyout) // checks if the flyout is still the current one
                {
                    flyout.Close();
                    _currentFlyout = null;
                }
            };
            timer.Start();

            // updates the current timer reference
            _currentTimer = timer;
        }

        private void CloseFlyout()
        {
            if (_currentFlyout is not null)
            {
                _currentFlyout.Close();
                _currentFlyout = null;
            }

            if (_currentTimer is not null)
            {
                _currentTimer.Stop();
                _currentTimer = null;
            }
        }

        /// <summary>
        /// Sets up a hook to intercept Windows messages and registers a global CTRL + C hotkey to process copy attempts.
        /// </summary>
        private void RegisterKeyboardCatching()
        {
            if (_userSettings.General.FlyoutsEnabled && _userSettings.Behavior.EnableKeyboardFlyouts)
            {
                var helper = new WindowInteropHelper(Application.Current.MainWindow);
                _source = HwndSource.FromHwnd(helper.Handle);
                _source.AddHook(WndProc);

                RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.C));
            }
        }

        /// <summary>
        /// Removes the hook to intercept Windows messages for processing keyboard copy attempts.
        /// </summary>
        /// <remarks>
        /// Public so that it can be triggered when closing.
        /// </remarks>
        public void Unregister()
        {
            CloseFlyout();
            _source?.RemoveHook(WndProc);
            UnregisterHotKey(new WindowInteropHelper(Application.Current.MainWindow).Handle, HOTKEY_ID);
        }

        ~CopyCatcher()
        {
            Unregister();
        }
    }
}
