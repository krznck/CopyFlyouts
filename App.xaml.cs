using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using log4net;
using log4net.Config;
using Microsoft.VisualBasic.Logging;

namespace copy_flyouts
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        private readonly string logFilePath;

        public App()
        {
            XmlConfigurator.Configure();
            logFilePath = GetLogFilePath();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private async void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;

            log.Error("Unhandled UI Exception", e.Exception);
            string message = "An unexpected error occurred. " + appName + " will close." +
                "\nLog file can be located at " + logFilePath;
            Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
            messageBox.Title = "Error";
            messageBox.Content = message;
            await messageBox.ShowDialogAsync();
            e.Handled = true;
            Current.Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("Unhandled non-UI Exception", e.ExceptionObject as Exception);
        }

        private string GetLogFilePath()
        {
            var logRepository = LogManager.GetRepository();
            var appenders = logRepository.GetAppenders();
            var fileAppender = appenders.OfType<log4net.Appender.FileAppender>().FirstOrDefault();
            return fileAppender?.File ?? "Log file path not found.";
        }
    }
}