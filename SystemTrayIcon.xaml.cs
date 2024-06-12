using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace copy_flyouts
{
    /// <summary>
    /// Interaction logic for SystemTrayIcon.xaml
    /// </summary>
    public partial class SystemTrayIcon : Window
    {
        private readonly MainWindow mainWindow; // here so that we can bring it up, and register/unregister the hotkey

        public SystemTrayIcon(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            if (mainWindow.HotkeyHandler.IsRegistered)
            {
                VisualizeHotkeyEnabled();
            }
            else
            {
                VisualizeHotkeyDisabled();
            }
        }

        private void VisualizeHotkeyEnabled()
        {
            TooltipIcon.Symbol = SymbolRegular.Play24;
            PopupActivityText.Text = "Disable";
            PopupActivitySymbol.Symbol = SymbolRegular.Pause24;
        }

        private void VisualizeHotkeyDisabled()
        {
            TooltipIcon.Symbol = SymbolRegular.Pause24;
            PopupActivityText.Text = "Enable";
            PopupActivitySymbol.Symbol = SymbolRegular.Play24;
        }

        private void PopupActivityClick(object sender, RoutedEventArgs e)
        {
            if (mainWindow.HotkeyHandler.IsRegistered)
            {
                mainWindow.HotkeyHandler.Unregister();
                VisualizeHotkeyDisabled();
            }
            else
            {
                mainWindow.HotkeyHandler.Register();
                VisualizeHotkeyEnabled();
            }
        }

        private void PopupExitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void PopupExpandClick(object sender, RoutedEventArgs e)
        {
            // gets rid of the remaining notify icon, since we don't need this until the main window is minimized again
            NotifyIcon.TrayPopupResolved.IsOpen = false;
            NotifyIcon.Dispose();

            // and brings the main window up
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }
}
