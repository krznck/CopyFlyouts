using System;
using System.Collections.Generic;
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

namespace copy_flash_wpf
{
    /// <summary>
    /// Interaction logic for SystemTrayIcon.xaml
    /// </summary>
    public partial class SystemTrayIcon : Window
    {
        public HotkeyHandler HotkeyHandler;

        public SystemTrayIcon(HotkeyHandler hotkeyHandler)
        {
            InitializeComponent();
            HotkeyHandler = hotkeyHandler;

            if (hotkeyHandler.IsRegistered)
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
            if (HotkeyHandler.IsRegistered)
            {
                HotkeyHandler.Unregister();
                VisualizeHotkeyDisabled();
            }
            else
            {
                HotkeyHandler.Register();
                VisualizeHotkeyEnabled();
            }
        }

        private void PopupExitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
