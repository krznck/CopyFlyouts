using copy_flyouts.UpdateInfrastructure;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq.Expressions;
using Wpf.Ui.Controls;
using System.Windows.Threading;
using copy_flyouts.Core;

namespace copy_flyouts.Pages
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        private UpdateChecker updateChecker;
        private Settings _userSettings;

        public About()
        {
            InitializeComponent();
            this.Loaded += About_Loaded;
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            _userSettings = DataContext as Settings;
            updateChecker = new UpdateChecker(_userSettings);
            RefreshUpdateStatusIndicators();
            _userSettings.PropertyChanged += UserSettings_PropertyChanged;

            LoadToggleLabels();
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.UpdatePageUrl):
                    RefreshUpdateStatusIndicators();
                    break;
                case nameof(Settings.AutoUpdate):
                    AutoUpdateLabel.Text = _userSettings.AutoUpdate ? "On" : "Off";
                    break;
                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            AutoUpdateLabel.Text = _userSettings.AutoUpdate ? "On" : "Off";
        }

        private void RefreshUpdateStatusIndicators()
        {
            if (_userSettings.UpdatePageUrl != null)
            {
                OpenUpdatePageButton.IsEnabled = true;
                VersionSecondaryText.Visibility = Visibility.Visible;
            }
            else
            {
                OpenUpdatePageButton.IsEnabled = false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await updateChecker.CheckForUpdatesManually();
        }

        private void OpenUpdatePageButton_Click(object sender, RoutedEventArgs e)
        {
            updateChecker.OpenUpdatePage();
        }
    }
}
