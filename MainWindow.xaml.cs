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

namespace copy_flash_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        HotkeyHandler hotkeyHandler;

        public MainWindow()
        {
            InitializeComponent();
            CreateNotifyIcon();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void CreateNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            var iconUri = new Uri("pack://application:,,,/assets/icons/ic_fluent_clipboard_checkmark_16_filled.ico", UriKind.RelativeOrAbsolute);
            var streamResourceInfo = System.Windows.Application.GetResourceStream(iconUri);
            if (streamResourceInfo != null)
            {
                using (var stream = streamResourceInfo.Stream)
                {
                    notifyIcon.Icon = new System.Drawing.Icon(stream);
                }
            }
            notifyIcon.DoubleClick += (s, args) => ShowWindow();
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, args) => Close());
            notifyIcon.Visible = true;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnClosing(e);
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hotkeyHandler = new(this);
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            hotkeyHandler.Unregister();
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }
    }
}