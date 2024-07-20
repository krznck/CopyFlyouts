using copy_flyouts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace copy_flyouts.UpdateInfrastructure
{
    public class UpdateChecker
    {
        private readonly HttpClient client = new HttpClient();
        // note - adding a token like this straight into source code is bad, but it will be fine so long as the repo is private.
        // by the time this repo is publicized, the token will be expired or deleted
        private readonly string personalAccessToken = "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";
        public readonly string currentVersion = "0.2.0";
        private readonly DispatcherTimer updateCheckTimer = new DispatcherTimer();

        public async Task<GitHubRelease> GetLatestReleaseAsync()
        {
            try
            {
                var url = $"https://api.github.com/repos/krznck/copy-flyouts/releases/latest";
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", personalAccessToken);
                var response = await client.GetStringAsync(url);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return release;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching release info: {ex.Message}");
                return null;
            }
        }

        public async Task CheckForUpdatesManually()
        {
            UpdateChecker updateChecker = new UpdateChecker();

            var latestRelease = await updateChecker.GetLatestReleaseAsync();

            if (latestRelease != null)
            {
                if (IsNewVersionAvailable(currentVersion, latestRelease.TagName))
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "New Updates";
                    messageBox.Content = $"A new version - {latestRelease.TagName} - is available on GitHub!";
                    messageBox.IsPrimaryButtonEnabled = true;
                    messageBox.PrimaryButtonText = "Open update page";
                    if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary)
                    {
                        Process.Start(new ProcessStartInfo(latestRelease.HtmlUrl) { UseShellExecute = true });
                    }
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "No Updates";
                    messageBox.Content = "You are using the latest version.";
                    var result = await messageBox.ShowDialogAsync();
                }
            }
            else
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = "Failed to check for updates.";
                await messageBox.ShowDialogAsync();
            }
        }

        public async Task CheckForUpdatesAutomatically()
        {
            UpdateChecker updateChecker = new UpdateChecker();

            var latestRelease = await updateChecker.GetLatestReleaseAsync();

            if ((latestRelease != null) && IsNewVersionAvailable(currentVersion, latestRelease.TagName))
            {                    
                new ToastContentBuilder()
                    .AddText("New update avaiable")
                    .AddText($"A new version is available!")
                    .AddButton(new ToastButton()
                        .SetContent("Open update page")
                        .SetProtocolActivation(new Uri(latestRelease.HtmlUrl)))
                    .Show();

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

        public void InitializeUpdateCheckTimer()
        {
            updateCheckTimer.Interval = TimeSpan.FromSeconds(15);
            updateCheckTimer.Tick += UpdateCheckTimer_Tick;
            updateCheckTimer.Start();
        }

        private async void UpdateCheckTimer_Tick(object sender, EventArgs e)
        {
            await CheckForUpdatesAutomatically();
        }
    }
}
