namespace CopyFlyouts
{
    using System.Runtime.InteropServices;
    using System.Windows;
    using log4net;
    using log4net.Config;

    /// <summary>
    /// Interaction logic for App.xaml
    /// Oversees starting the application and handling the logging and Mutex logic.
    /// </summary>
    public partial class App : Application
    {
        // crash logging stuff
        private readonly ILog _log = LogManager.GetLogger(typeof(App));
        private readonly string _logFilePath;

        private Mutex? _mutex = null; // ensures there is one instance of the program
        public const int WM_SHOWAPP = 0x0400; // WM_USER

        // these methods are imported to ensure we can send a message to show the original process' MainWindow
        // when this process turns out to be a duplicate
        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            XmlConfigurator.Configure();
            _logFilePath = GetLogFilePath();
        }

        /// <summary>
        /// Overriden OnStartup behavior to check for Mutex availability, then if the Mutex is already up,
        /// show the <see cref="CopyFlyouts.MainWindow"/> of the original instance, and shut down this one.
        /// Additionally, sets up unhandled exception event handlers for logging.
        /// </summary>
        /// <param name="e">Startup event arguments - minor role in debugging only if resources can't be found.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (
                Current.Resources["ProgramName"] is not string appName
                || Current.Resources["AuthorName"] is not string authorName
                || Current.Resources["Version"] is not string version
            )
            {
                throw new ArgumentNullException(
                    "Can't find program naming resources on StartUp with arguments: " + e
                );
            }

            string mutexName = authorName + "." + appName + "." + version + ".Mutex";

            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew) // instance of this program is already running
            {
                nint hWnd = FindWindow(null, appName); // so find its window
                if (hWnd != nint.Zero)
                {
                    SendMessage(hWnd, WM_SHOWAPP, nint.Zero, nint.Zero); // and tell it to show itself
                }

                Environment.Exit(0); // then just close this with no futher startup procedures
            }

            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Overriden OnExit behavior to ensure that the mutex is released before exit.
        /// </summary>
        /// <param name="e">Exit event arguments (unused).</param>
        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex is not null)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
            }

            base.OnExit(e);
        }

        private async void App_DispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e
        )
        {
            string? appName = Current.Resources["ProgramName"] as string;

            _log.Error("Unhandled UI Exception", e.Exception);

            // we inform the user that the program has crashed
            string message =
                $"An unexpected error occurred. {appName} will close.\nLog file can be located at {_logFilePath}";

            Wpf.Ui.Controls.MessageBox messageBox = new() { Title = "Error", Content = message, };
            await messageBox.ShowDialogAsync();

            e.Handled = true;
            Current.Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("Unhandled non-UI Exception", e.ExceptionObject as Exception);
        }

        private static string GetLogFilePath()
        {
            var logRepository = LogManager.GetRepository();
            var appenders = logRepository.GetAppenders();
            var fileAppender = appenders.OfType<log4net.Appender.FileAppender>().FirstOrDefault();
            return fileAppender?.File ?? "Log file path not found.";
        }
    }
}
