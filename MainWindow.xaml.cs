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
using Hardcodet.Wpf.TaskbarNotification;

namespace copy_flash_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SystemTrayIcon notifyIcon;
        private HotkeyHandler hotkeyHandler;
        public HotkeyHandler HotkeyHandler { get => hotkeyHandler; private set => hotkeyHandler = value; }

        public MainWindow()
        {
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
            if (WindowState == WindowState.Minimized)
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
            HotkeyHandler = new(this);
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotkeyHandler.Unregister();
            System.Windows.Application.Current.Shutdown();
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }
    }
}