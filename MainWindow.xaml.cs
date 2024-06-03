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
        HotkeyHandler hotkeyHandler;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
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
            string clipboard = System.Windows.Clipboard.GetText();
            copyText.Text = clipboard;

            var flyout = new Flyout(clipboard.Trim());
            flyout.Show();
            await Task.Delay(1500);
            flyout.Close();
        }
    }
}