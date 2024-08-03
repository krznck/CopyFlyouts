﻿using System.Diagnostics;
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
using System.Windows.Threading;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using copy_flyouts.Core;
using Microsoft.Win32;
using copy_flyouts.UpdateInfrastructure;
using copy_flyouts.Pages;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media.Animation;
using WK.Libraries.SharpClipboardNS;
using System.IO;
using copy_flyouts.Resources;

namespace copy_flyouts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private HotkeyHandler hotkeyHandler;
        public HotkeyHandler HotkeyHandler { get => hotkeyHandler; private set => hotkeyHandler = value; }
        public Settings UserSettings { get; set; }
        private UpdateChecker updateChecker;
        private DummyDataHolder dummyDataHolder = new DummyDataHolder();

        public MainWindow()
        {
            // before we show the window, we make sure the program matches the system theme
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
                // also changes the resource dictionary
            var newTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
            string theme = newTheme.ToString();
            string newThemeDictionaryPath = "Resources/" + theme + ".xaml";
            ResourceDictionary newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri(newThemeDictionaryPath, UriKind.Relative)
            };
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(newThemeDictionary);

            UserSettings = new Settings();
            updateChecker = new UpdateChecker(UserSettings);

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // when the flyouts are disabled, but on a change DIFFERENT than disabling them
            if (e.PropertyName != nameof(UserSettings.FlyoutsEnabled) && !UserSettings.FlyoutsEnabled)
            {
                WarningFooter.Visibility = Visibility.Visible; // we show the footer to warn the user
            }

            if (e.PropertyName == nameof(UserSettings.Theme))
            {
                if (UserSettings.Theme.Equals("Light"))
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(this);
                }
                else if (UserSettings.Theme.Equals("Dark"))
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(this);
                }
                else if (UserSettings.Theme.Equals("System"))
                {
                    ApplicationThemeManager.ApplySystemTheme();
                    Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
                }

                RefreshAboutPageAttentionStatus(); // this is here to ensure the color is correct to the theme
            }

            if (e.PropertyName == nameof(UserSettings.FlyoutsEnabled))
            {
                if (UserSettings.FlyoutsEnabled)
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
            }

            if (e.PropertyName == nameof(UserSettings.RunOnStartup))
            {
                if (UserSettings.RunOnStartup)
                {
                    AddToStartup();
                }
                else
                {
                    RemoveFromStartup();
                }
            }

            if (e.PropertyName == nameof(UserSettings.UpdatePageUrl))
            {
                RefreshAboutPageAttentionStatus();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && UserSettings.MinimizeToTray)
            {
                Hide();
                notifyIcon.Visibility = Visibility.Visible;
                if (!notifyIcon.IsRegistered)
                {
                    notifyIcon.Register();
                }

                NotifyAboutMinimization();
            }
            base.OnStateChanged(e);
        }

        private void NotifyAboutMinimization()
        {
            if (UserSettings.NotifyAboutMinimization)
            {
                var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;

                new ToastContentBuilder()
                    .AddText($"Minimized to system tray")
                    .AddText($"{appName} will run in the background, and can still be accessed from the system tray!")
                    .Show();
            }

        }

        public void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            notifyIcon.Unregister();
            notifyIcon.Visibility = Visibility.Collapsed;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void RefreshNotifyIconAppearance()
        {
            Uri iconUri;
            string filePath = "pack://application:,,,/Assets/icons/";

            // without this update, the program will think the old theme is still up
            SystemThemeManager.UpdateSystemThemeCache();

            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark)
            {
                if (UserSettings.FlyoutsEnabled)
                {
                    filePath += "logo-slim-darkmode";
                }
                else
                {
                    filePath += "logo-slim-darkmode-disabled";
                }
            }
            else
            {
                if (UserSettings.FlyoutsEnabled)
                {
                    filePath += "logo-slim";
                }
                else
                {
                    filePath += "logo-slim-disabled";
                }
            }

            filePath += ".ico";
            //Debug.WriteLine(filePath);
            iconUri = new Uri(filePath, UriKind.RelativeOrAbsolute);
            ImageSource icon = new BitmapImage(iconUri);
            notifyIcon.Icon = icon;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HotkeyHandler = new(this, UserSettings); // creates the hotkey to make the program work

            // handles switching the theme when the system does
            Wpf.Ui.Appearance.ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged; // this is to changing the notifyicon when the system theme changes

            // and ensures that the correct theme is applied on load
            if (UserSettings.Theme.Equals("Dark"))
            {
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            }
            else if (UserSettings.Theme.Equals("Light"))
            {
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
            }
            else if (UserSettings.Theme.Equals("System"))
            {
                ApplicationThemeManager.ApplySystemTheme();
                Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            }

            RootNavigation.DataContext = UserSettings;
            RootNavigation.Navigate(typeof(Pages.General)); // ensures General page is opened on load

            RefreshNotifyIconAppearance();

            notifyIcon.Unregister(); // note: we can't just set the initial Visibility to Collapsed, otherwise the tooltip will always be invisible
            notifyIcon.Visibility = Visibility.Collapsed;

            if (UserSettings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                notifyIcon.Register();
                notifyIcon.Visibility = Visibility.Visible;
            }

            if (UserSettings.RunOnStartup)
            {
                AddToStartup();
            }
            else
            {
                RemoveFromStartup();
            }

            RefreshAboutPageAttentionStatus();

            if (!(UserSettings.UpdatePageUrl is null)) { updateChecker.NotifyAboutUpdate(); }

            ToolboxTextBox.Text = dummyDataHolder.CurrentText;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Category is Desktop when we force theme via Auto Dark Mode app, and General if we do it via the settings
            // beats me as to why--
            if (e.Category == UserPreferenceCategory.Desktop || e.Category == UserPreferenceCategory.General)
            {
                RefreshNotifyIconAppearance();
                RefreshAboutPageAttentionStatus();
            }
        }

        private void AddToStartup()
        {
            var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;
            var appFileName = appName + ".exe";

            string executablePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appFileName);
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey.SetValue(appName, executablePath);
        }

        private void RemoveFromStartup()
        {
            var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey.GetValue(appName) != null)
            {
                registryKey.DeleteValue(appName, false);
            }
        }

        private void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
        {
            var newTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
            // determines what the old theme was
            Uri oldThemeUri;
            if (newTheme.Equals(Wpf.Ui.Appearance.ApplicationTheme.Light))
            {
                oldThemeUri = new Uri("Resources/Dark.xaml", UriKind.Relative);
            }
            else
            {
                oldThemeUri = new Uri("Resources/Light.xaml", UriKind.Relative);
            }

            // find it by its uri
            var oldThemeDictionary = System.Windows.Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source == oldThemeUri);

            // removes it, if found
            if (oldThemeDictionary != null)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Remove(oldThemeDictionary);
            }

            // and adds the new one in
            string theme = newTheme.ToString();
            string newThemeDictionaryPath = "Resources/" + theme + ".xaml";
            ResourceDictionary newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri(newThemeDictionaryPath, UriKind.Relative)
            };
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(newThemeDictionary);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UserSettings.MinimizeOnClosure && UserSettings.MinimizeToTray)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else
            {
                HotkeyHandler.Unregister();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void notifyIcon_LeftClick(Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ProgramStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (UserSettings.FlyoutsEnabled)
            {
                UserSettings.FlyoutsEnabled = false;
            }
            else
            {
                UserSettings.FlyoutsEnabled = true;
            }
        }

        private void RefreshAboutPageAttentionStatus()
        {
            if (UserSettings.UpdatePageUrl != null)
            {
                AboutSymbol.Filled = true;
                SolidColorBrush colorStatusDangerForeground1 = (SolidColorBrush)System.Windows.Application.Current.Resources["colorStatusDangerForeground1"];
                AboutSymbol.Foreground = colorStatusDangerForeground1;
            }
            else
            {
                AboutSymbol.Filled = false;
                AboutSymbol.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            }
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
                dummyDataHolder.ResetText();
                ToolboxTextBox.Text = dummyDataHolder.CurrentText;

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
            // Place the BitmapSource on the clipboard
            System.Windows.Forms.Clipboard.SetDataObject(dummyDataHolder.CurrentImage, false, 5, 200);

            ToolboxImageButton.IsEnabled = false;
            await Task.Delay(250);
            ToolboxImageButton.IsEnabled = true;
        }

        private void ToolboxRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            dummyDataHolder.Refresh();
            ToolboxTextBox.Text = dummyDataHolder.CurrentText;
        }
    }
}