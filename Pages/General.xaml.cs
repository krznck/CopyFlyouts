using copy_flyouts.Core;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace copy_flyouts.Pages
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : Page
    {
        private Settings? _userSettings;

        public General()
        {
            InitializeComponent();
            Loaded += General_Loaded;
        }

        private void General_Loaded(object sender, RoutedEventArgs e)
        {
            _userSettings = DataContext as Settings;
            _userSettings.PropertyChanged += _userSettings_PropertyChanged;

            LoadToggleLabels();
        }

        private void _userSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.FlyoutsEnabled):
                    EnableCopyFlyoutsLabel.Text = _userSettings.FlyoutsEnabled ? "On" : "Off";
                    break;
                case nameof(Settings.RunOnStartup):
                    RunOnStartupLabel.Text = _userSettings.RunOnStartup ? "On" : "Off";
                    break;
                case nameof(Settings.StartMinimized):
                    StartMinimizedLabel.Text = _userSettings.StartMinimized ? "On" : "Off";
                    break;
                case nameof(Settings.MinimizeToTray):
                    MinimizeToTrayLabel.Text = _userSettings.MinimizeToTray ? "On" : "Off";
                    NotifyMinimizedLabel.Appearance = _userSettings.MinimizeToTray ? Wpf.Ui.Controls.TextColor.Primary : Wpf.Ui.Controls.TextColor.Secondary;
                    break;
                case nameof(Settings.NotifyAboutMinimization):
                    NotifyMinimizedLabel.Text = _userSettings.NotifyAboutMinimization ? "On" : "Off";
                    break;
                case nameof(Settings.AutoUpdate):
                    CheckForUpdatesLabel.Text = _userSettings.AutoUpdate ? "On" : "Off";
                    break;
                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            EnableCopyFlyoutsLabel.Text = _userSettings.FlyoutsEnabled ? "On" : "Off";
            RunOnStartupLabel.Text = _userSettings.RunOnStartup ? "On" : "Off";
            StartMinimizedLabel.Text = _userSettings.StartMinimized ? "On" : "Off";
            MinimizeToTrayLabel.Text = _userSettings.MinimizeToTray ? "On" : "Off";
            NotifyMinimizedLabel.Text = _userSettings.NotifyAboutMinimization ? "On" : "Off";
            NotifyMinimizedLabel.Appearance = _userSettings.MinimizeToTray ? Wpf.Ui.Controls.TextColor.Primary : Wpf.Ui.Controls.TextColor.Secondary;
            CheckForUpdatesLabel.Text = _userSettings.AutoUpdate ? "On" : "Off";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_userSettings != null)
            {
                // to be honest, I couldn't easily think of a good way to get the default values of those attributes somehow
                // so screw it, just hardcode them in
                _userSettings.FlyoutsEnabled = true;
                _userSettings.StartMinimized = false;
                _userSettings.MinimizeToTray = true;
                _userSettings.RunOnStartup = false;
                _userSettings.AutoUpdate = true;
            }
        }
    }
}
