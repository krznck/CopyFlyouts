using copy_flyouts.Core;
using copy_flyouts.Properties;
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
    /// Interaction logic for Appearance.xaml
    /// </summary>
    public partial class Appearance : Page
    {
        private Core.Settings? _userSettings;

        public Appearance()
        {
            InitializeComponent();

            ThemeComboBox.ItemsSource = new List<string>
            {
                "System",
                "Light",
                "Dark"
            };

            Loaded += Appearance_Loaded;
        }

        private void Appearance_Loaded(object sender, RoutedEventArgs e)
        {
            _userSettings = DataContext as Core.Settings;
            _userSettings.PropertyChanged += _userSettings_PropertyChanged;

            LoadToggleLabels();
        }

        private void _userSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Core.Settings.InvertedTheme):
                    InvertedThemeLabel.Text = _userSettings.InvertedTheme ? "On" : "Off";
                    break;
                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            InvertedThemeLabel.Text = _userSettings.InvertedTheme ? "On" : "Off";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Core.Settings;

            if (userSettings != null)
            {
                // to be honest, I couldn't easily think of a good way to get the default values of those attributes somehow
                // so screw it, just hardcode them in
                userSettings.FlyoutOpacity = 1.0;
                userSettings.FlyoutWidthScale = 1.0;
                userSettings.FlyoutHeightScale = 1.0;
                userSettings.FlyoutFontSizeScale = 1.0;
                userSettings.Theme = "System";
                userSettings.InvertedTheme = false;
                userSettings.FlyoutCorners = 5;
            }
        }
    }
}
