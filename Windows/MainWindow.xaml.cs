namespace CopyFlyouts
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Wpf.Ui.Controls;
    using Wpf.Ui.Appearance;
    using Microsoft.Win32;
    using Microsoft.Toolkit.Uwp.Notifications;
    using CopyFlyouts.Core;
    using CopyFlyouts.UpdateInfrastructure;
    using CopyFlyouts.Resources;
    using CopyFlyouts.Pages;
    using CopyFlyouts.Settings;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Acts as the main logic of the program from which other classes flow.
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private CopyCatcher? _copyCatcher;
        private readonly SettingsManager _userSettings;
        private readonly UpdateChecker _updateChecker;
        private readonly DummyDataHolder _dummyDataHolder = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up additional event handlers related to the window and the settings.
        /// </summary>
        public MainWindow()
        {
            _userSettings = new SettingsManager();
            _userSettings.LoadSettingsFile();
            _updateChecker = new UpdateChecker(_userSettings.About);

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            _userSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        #region Non-click Event Handlers

        /// <summary>
        /// Overriden OnStateChanged behavior to ensure window is minimized to system tray when the appropriate setting is enabled.
        /// </summary>
        /// <param name="e">Event argument (unused).</param>
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _userSettings.General.MinimizeToTray)
            {
                Hide();
                NotifyIcon.Visibility = Visibility.Visible;

                if (!NotifyIcon.IsRegistered) { NotifyIcon.Register(); }

                NotifyAboutMinimization();
            }

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Handles the Loaded event for the <see cref="MainWindow"/> instance.
        /// </summary>
        /// <remarks>
        /// Initializes the <see cref="CopyCatcher"/>, ensures proper themeing before the window is shown, proper handling of pages,
        /// preparing the system tray icon and showing it if the user wants the window to be minimized, handling startup behavior,
        /// proper update indicator before the window is shown, and default toolbox instructions.
        /// </remarks>
        /// <param name="sender">Source of the event - typically the MainWindow instance.</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _copyCatcher = new(_userSettings); // this is the fundamental piece of the program, together with Flyout windows
            // note: it is crucial to do this on load and not initialization to avoid System.ArgumentException "hwnd of zero is not valid"

            #region Theme Handling
            // handles switching the theme when the system does
            ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;
            // we also have to listen to the system preferences to apply custom changes to the system tray icon
            // (essentially our own way of watching the system theme, just for that)
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            // and ensures that the correct theme is applied on load
            if (_userSettings.Appearance.Theme.Equals("Dark")) { ApplicationThemeManager.Apply(ApplicationTheme.Dark); }
            else if (_userSettings.Appearance.Theme.Equals("Light")) { ApplicationThemeManager.Apply(ApplicationTheme.Light); }
            else if (_userSettings.Appearance.Theme.Equals("System"))
            {
                ApplicationThemeManager.ApplySystemTheme();
                SystemThemeWatcher.Watch(this);
            }

            // and changes the resource dictionary to allign with the app theme
            var newTheme = ApplicationThemeManager.GetAppTheme();
            string newThemeDictionaryPath = "Resources/" + newTheme.ToString() + ".xaml";
            ResourceDictionary newThemeDictionary = new()
            {
                Source = new Uri(newThemeDictionaryPath, UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(newThemeDictionary);

            RefreshNotifyIconAppearance(); // finally, we ensure the notify icon will use the correct team when visible
            #endregion

            // all pages need to use the user settings extensively, and we can pass it to all of them universally like this
            RootNavigation.DataContext = _userSettings;
            RootNavigation.Navigate(typeof(General)); // ensures General page is opened on load
            RootNavigation.Navigated += RootNavigation_Navigated;

            // prepares the system tray icon
            // note: we can't just set the initial Visibility to Collapsed while keeping it registered,
            // otherwise the tooltip will always be invisible
            NotifyIcon.Unregister();
            NotifyIcon.Visibility = Visibility.Collapsed;

            if (_userSettings.General.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                // but we make it visible right away if the program starts minimized
                NotifyIcon.Register();
                NotifyIcon.Visibility = Visibility.Visible;
            }

            // these are here in the rare case that the user manually changed the settings file while the program was off,
            // in which case the OnPropertyChanged of the user settings would have never triggered to add it/remove it from startup
            if (_userSettings.General.RunOnStartup) { AddToStartup(); }
            else { RemoveFromStartup(); }

            // we want to make sure it's clear when there is an update, so it's good to do these on each load
            RefreshUpdateIndicator();
            if (_userSettings.About.UpdatePageUrl is not null) { UpdateChecker.NotifyAboutUpdate(); }

            ToolboxTextBox.Text = _dummyDataHolder.CurrentText;
        }

        /// <summary>
        /// Handles the Closing event for the <see cref="MainWindow"/> instance.
        /// </summary>
        /// <remarks>
        /// Ensures that if the user wants the close button to minimize the program (which is pretty common), then that's what it does.
        /// Additionally, unregisters the <see cref="CopyCatcher"/> instance before shutting down.
        /// </remarks>
        /// <param name="sender">Source of the event - typically the <see cref="MainWindow"/> instance.</param>
        /// <param name="e">Cancel event arguments - used to cancel closing if the user configured the settings that way.</param>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_userSettings.General.MinimizeOnClosure && _userSettings.General.MinimizeToTray)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else
            {
                _copyCatcher?.Unregister();
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Handles the Navigated event of the <see cref="RootNavigation"/> <see cref="NavigationView"/>.
        /// Namely, passes the UpdateChecker to the <see cref="About"/> page, if that is the page being navigated to.
        /// </summary>
        /// <remarks>
        /// This is to ensure we don't unnecessarily create multiple instances of <see cref="UpdateChecker"/> just so
        /// the <see cref="About"/> page has access to some of its methods, since the <see cref="UpdateChecker"/> is capable
        /// of doing things automatically, and it would be wasteful or buggy to do the same thing multiple times.
        /// </remarks>
        /// <param name="sender">Sender of the event, in this case <see cref="NavigationView"/> </param>
        /// <param name="args">Navigated events arguments - used here to check whether the page being navigated to is <see cref="About"/></param>
        private void RootNavigation_Navigated(NavigationView sender, NavigatedEventArgs args)
        {
            if (args.Page is About aboutPage && aboutPage.UpdateChecker is null) { aboutPage.UpdateChecker = _updateChecker; }
        }

        /// <summary>
        /// Event handler responsible for switching the Resources.Dark.xaml and Resources.Light.xaml to match the theme of the app.
        /// </summary>
        /// <param name="currentApplicationTheme">The application theme after the change - used to delete the old resource and switch to the new.</param>
        /// <param name="systemAccent">System accent color (unused).</param>
        private void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            // determines what the old theme was
            Uri oldThemeUri;
            if (currentApplicationTheme.Equals(ApplicationTheme.Light)) { oldThemeUri = new Uri("Resources/Dark.xaml", UriKind.Relative); }
            else { oldThemeUri = new Uri("Resources/Light.xaml", UriKind.Relative); }

            // finds it by its uri
            var oldThemeDictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == oldThemeUri);

            // removes it, if found
            if (oldThemeDictionary is not null) { Application.Current.Resources.MergedDictionaries.Remove(oldThemeDictionary); }

            // and adds the new one in
            string newThemeDictionaryPath = "Resources/" + currentApplicationTheme.ToString() + ".xaml";
            ResourceDictionary newThemeDictionary = new () { Source = new Uri(newThemeDictionaryPath, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(newThemeDictionary);

            // and finally also updates the update indicator with the new theme colors
            RefreshUpdateIndicator();
        }

        /// <summary>
        /// Handles changing the system tray icon to match the system theme. Essentially a simple system theme watcher.
        /// It is important it should not match the application theme, since naturally the application theme does not change the taskbar.
        /// </summary>
        /// <param name="sender">Source of the event - typically <see cref="Microsoft.Win32.SystemEvents"/> in this case.</param>
        /// <param name="e">User preferences changed arguments (unused).</param>
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Category is Desktop when we force theme via the Auto Dark Mode app, and General if we do it via the settings
            // I don't really know why
            if (e.Category == UserPreferenceCategory.Desktop || e.Category == UserPreferenceCategory.General)
            {
                RefreshNotifyIconAppearance();
                RefreshUpdateIndicator();
            }
        }

        /// <summary>
        /// Handles behavior for when user settings have changed.
        /// </summary>
        /// <param name="sender">Source of the event - should be <see cref="SettingsManager"/> (unused).</param>
        /// <param name="e">Property changed event arguments - used to see which property has changed.</param>
        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // when the flyouts are disabled, but on a change DIFFERENT than disabling them
            if (e.PropertyName != nameof(SettingsManager.General.FlyoutsEnabled) && !_userSettings.General.FlyoutsEnabled)
            {
                WarningFooter.Visibility = Visibility.Visible; // we show the footer to warn the user
            }

            switch (e.PropertyName)
            {
                case nameof(SettingsManager.Appearance.Theme):
                    if (_userSettings.Appearance.Theme.Equals("Light"))
                    {
                        ApplicationThemeManager.Apply(ApplicationTheme.Light);
                        SystemThemeWatcher.UnWatch(this);
                    }
                    else if (_userSettings.Appearance.Theme.Equals("Dark"))
                    {
                        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                        SystemThemeWatcher.UnWatch(this);
                    }
                    else if (_userSettings.Appearance.Theme.Equals("System"))
                    {
                        ApplicationThemeManager.ApplySystemTheme();
                        SystemThemeWatcher.Watch(this);
                    }

                    RefreshUpdateIndicator(); // this is here to ensure the color is correct to the theme
                    break;

                case nameof(SettingsManager.General.FlyoutsEnabled): // the system tray icon has to be updated
                    if (_userSettings.General.FlyoutsEnabled)
                    {
                        ProgramStateMenuItem.Header = "Disable";
                        ProgramStateMenuItem.Icon = new SymbolIcon { Symbol = SymbolRegular.Play24 };
                        WarningFooter.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ProgramStateMenuItem.Header = "Enable";
                        ProgramStateMenuItem.Icon = new SymbolIcon { Symbol = SymbolRegular.Pause24 };
                    }

                    RefreshNotifyIconAppearance();
                    break;

                case nameof(SettingsManager.General.RunOnStartup):
                    if (_userSettings.General.RunOnStartup) { AddToStartup(); }
                    else { RemoveFromStartup(); }
                    break;

                case nameof(SettingsManager.About.UpdatePageUrl):
                    RefreshUpdateIndicator();
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region Startup

        /// <summary>
        /// Adds the program to user's startup programs.
        /// </summary>
        /// <remarks>
        /// This is done by adding the executable absolute path to the user's SOFTWARE\Microsoft\Windows\CurrentVersion\Run registry.
        /// </remarks>
        private static void AddToStartup()
        {
            var appName = Application.Current.Resources["ProgramName"] as string;
            var appFileName = appName + ".exe";

            string executablePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appFileName);
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey?.SetValue(appName, executablePath);
        }

        /// <summary>
        /// Removes the program from the user's startup programs.
        /// </summary>
        /// <remarks>
        /// This is done by removing the executable path from the user's SOFTWARE\Microsoft\Windows\CurrentVersion\Run registry.
        /// </remarks>
        private static void RemoveFromStartup()
        {
            var appName = Application.Current.Resources["ProgramName"] as string;

            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (registryKey is not null
                && registryKey.GetValue(appName) is not null
                && appName is not null)
            {
                registryKey.DeleteValue(appName, false);
            }
        }

        #endregion

        /// <summary>
        /// Displays a notification toast informing the user that the program has been minimized to the system tray.
        /// </summary>
        /// <remarks>
        /// Only happens if the "Notify when minimized to system tray" setting is on.
        /// </remarks>
        private void NotifyAboutMinimization()
        {
            if (_userSettings.General.NotifyAboutMinimization)
            {
                var appName = Application.Current.Resources["ProgramName"] as string;

                new ToastContentBuilder()
                    .AddText($"Minimized to system tray")
                    .AddText($"{appName} will run in the background, and can still be accessed from the system tray!")
                    .Show();
            }

        }

        /// <summary>
        /// Shows the <see cref="MainWindow"/> instance, and makes the system tray icon disappear.
        /// </summary>
        /// <remarks>
        /// <see cref="Window.Show()"/> cannot be overriden, hence method hiding was used instead.
        /// </remarks>
        private new void Show()
        {
            base.Show();
            WindowState = WindowState.Normal;
            NotifyIcon.Unregister();
            NotifyIcon.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Updates the system tray icon to allign with the current theme of the system,
        /// by supplying it a different image from an asset file path.
        /// </summary>
        private void RefreshNotifyIconAppearance()
        {
            Uri iconUri;
            string filePath = "pack://application:,,,/Assets/icons/";

            // without this update, the program will think the old theme is still up
            SystemThemeManager.UpdateSystemThemeCache();

            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark)
            {
                filePath += _userSettings.General.FlyoutsEnabled ? "logo-slim-darkmode" : "logo-slim-darkmode-disabled";
            }
            else
            {
                filePath += _userSettings.General.FlyoutsEnabled ? "logo-slim" : "logo-slim-disabled";
            }

            filePath += ".ico";
            iconUri = new Uri(filePath, UriKind.RelativeOrAbsolute);
            ImageSource icon = new BitmapImage(iconUri);
            NotifyIcon.Icon = icon;
        }

        /// <summary>
        /// Sets the update indicator, responsible for informing the user there's an update available within the settings,
        /// to either show itself, or stay empty.
        /// </summary>
        private void RefreshUpdateIndicator()
        {
            if (_userSettings.About.UpdatePageUrl is not null)
            {
                AboutSymbol.Filled = true;
                SolidColorBrush colorStatusDangerForeground1 = (SolidColorBrush)Application.Current.Resources["colorStatusDangerForeground1"];
                AboutSymbol.Foreground = colorStatusDangerForeground1;
            }
            else
            {
                AboutSymbol.Filled = false;
                AboutSymbol.ClearValue(ForegroundProperty);
            }
        }

        #region Click Event Handlers

        private void NotifyIcon_LeftClick(Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            Show();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ProgramStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _userSettings.General.FlyoutsEnabled = !_userSettings.General.FlyoutsEnabled;
        }

        private void ToolboxChevron_Click(object sender, RoutedEventArgs e)
        {
            if (ToolboxMenu.Visibility == Visibility.Visible)
            {
                ToolboxMenu.Visibility = Visibility.Collapsed;
                ToolboxChevron.Icon = new SymbolIcon { Symbol = SymbolRegular.ChevronDoubleDown20 };
            }
            else
            {
                _dummyDataHolder.ResetText(); // we reset this in case the user needs instructions again
                ToolboxTextBox.Text = _dummyDataHolder.CurrentText;

                ToolboxMenu.Visibility = Visibility.Visible;
                ToolboxChevron.Icon = new SymbolIcon { Symbol = SymbolRegular.ChevronDoubleUp20 };
            }
        }

        private async void ToolboxCopyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(ToolboxTextBox.Text, false, 5, 200);

            // the clipboard really doesn't like getting spammed, due to being a shared resource,
            // and this is the safest way I've found to just not let that happen - we disable the button for a short time
            ToolboxCopyButton.IsEnabled = false;
            await Task.Delay(250);
            ToolboxCopyButton.IsEnabled = true;
        }

        private async void ToolboxImageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(_dummyDataHolder.CurrentImage, false, 5, 200);

            ToolboxImageButton.IsEnabled = false;
            await Task.Delay(250);
            ToolboxImageButton.IsEnabled = true;
        }

        private void ToolboxRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _dummyDataHolder.Refresh();
            ToolboxTextBox.Text = _dummyDataHolder.CurrentText; // only element that needs to be changed live
        }

        private async void ToolboxFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dataObject = new System.Windows.Forms.DataObject();
            dataObject.SetFileDropList(_dummyDataHolder.CurrentFiles);

            System.Windows.Forms.Clipboard.SetDataObject(dataObject, false, 5, 200);

            ToolboxFileButton.IsEnabled = false;
            await Task.Delay(250);
            ToolboxFileButton.IsEnabled = true;
        }

        #endregion
    }
}