namespace CopyFlyouts
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            XmlConfigurator.Configure();
            _logFilePath = GetLogFilePath();
        }

        /// <summary>
        /// Overriden OnStartup behavior to check for Mutex availability and shutdown the program if an instance is already running.
        /// Additionally, sets up unhandled exception event handlers for logging.
        /// </summary>
        /// <param name="e">Startup event arguments (unused).</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            string? appName = Current.Resources["ProgramName"] as string;
            string? authorName = Current.Resources["AuthorName"] as string;
            string? version = Current.Resources["Version"] as string;
            string mutexName = authorName + "." + appName + "." + version + ".Mutex";

            _mutex = new Mutex(true, mutexName, out bool createdNew);

            // an instance of this program is already running, so shut this down right now - no further startup procedures
            if (!createdNew) { Environment.Exit(0); }

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