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
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

namespace copy_flyouts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private SystemTrayIcon notifyIcon;
        private HotkeyHandler hotkeyHandler;
        public HotkeyHandler HotkeyHandler { get => hotkeyHandler; private set => hotkeyHandler = value; }
        public Settings DefaultSettings { get; private set; }
        public Settings UserSettings { get; set; }

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

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void CreateNotifyIcon()
        {
            notifyIcon = new SystemTrayIcon(this);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && UserSettings.MinimizeToTray)
            {
                Hide();
                CreateNotifyIcon();
            }
            base.OnStateChanged(e);
        }

        public void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HotkeyHandler = new(this, UserSettings); // creates the hotkey to make the program work

            // handles switching the theme when the system does
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            Wpf.Ui.Appearance.ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;

            RootNavigation.DataContext = UserSettings;
            RootNavigation.Navigate(typeof(Pages.General)); // ensures General page is opened on load

            if (UserSettings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
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

            if (notifyIcon != null) // and if there's a system tray icon at this point
            {
                notifyIcon.NotifyIcon.Dispose(); // then we remove it
                notifyIcon = new SystemTrayIcon(this); // and replace it with a new one
                // which ensures that the system tray icon also gets the refreshed theme
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotkeyHandler.Unregister();
            System.Windows.Application.Current.Shutdown();
        }
    }
}