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

namespace copy_flyouts.Pages
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        string currentVersion = "0.0.1";

        public About()
        {
            InitializeComponent();
            VersionLabel.Text = "Version " + currentVersion;
        }

        private async void CheckForUpdates()
        {
            UpdateChecker updateChecker = new UpdateChecker();

            var latestRelease = await updateChecker.GetLatestReleaseAsync();

            if (latestRelease != null)
            {
                if (IsNewVersionAvailable(currentVersion, latestRelease.TagName))
                {
                    System.Windows.MessageBox.Show($"A new version ({latestRelease.TagName}) is available! Visit {latestRelease.HtmlUrl} to download it.", "Update Available", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show($"You are using the latest version.", "No Updates", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to check for updates.", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            if (latestVersion.StartsWith("v"))
            {
                latestVersion = latestVersion.Substring(1);
            }

            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);
            return latest.CompareTo(current) > 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }
    }
}
