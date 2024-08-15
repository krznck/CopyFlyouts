namespace CopyFlyouts.UpdateInfrastructure
{
    using CopyFlyouts.Resources;
    using System.Net.Http;
    using System.Text.Json;
    using System.Windows.Threading;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Toolkit.Uwp.Notifications;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Responsible for enabling manual and automatic update checking,
    /// which comes in the form of comparing current program version with latest GitHub release version.
    /// </summary>
    public class UpdateChecker
    {
        private readonly AboutSettings _userAboutSettings;
        private readonly HttpClient _client = new ();
        // note - adding a token like this straight into source code is bad, but it will be fine so long as the repo is private.
        // by the time this repo is publicized, the token will be expired or deleted
        private readonly string _personalAccessToken = "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";
        private readonly string? _currentVersion;
        private readonly DispatcherTimer _updateCheckTimer = new ();
        // cached instead of creating a new one every time GetLatestReleaseAsync() is called
        private static readonly JsonSerializerOptions _jsonOptions = new () { PropertyNameCaseInsensitive = true }; 

        /// <summary>
        /// Initializes the <see cref="UpdateChecker"/> instance, taking the current version from Resources/CommonResources.xaml
        /// </summary>
        /// <param name="aboutSettings">The user <see cref="AboutSettings"/> object, containing user preferences to change object behavior.</param>
        public UpdateChecker(AboutSettings aboutSettings)
        {
            _userAboutSettings = aboutSettings;

            var commonResources = new ResourceDictionary
            {
                Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative)
            };
            _currentVersion = commonResources["Version"] as string;
            _currentVersion = _currentVersion?[1..];

            aboutSettings.PropertyChanged += UserSettings_PropertyChanged;

            // checks for updates on startup, before the timer starts
            if (aboutSettings.UpdateVersion is null && aboutSettings.AutoUpdate) { Task task = CheckForUpdatesAutomatically(); }
            InitializeUpdateCheckTimer();
        }

        /// <summary>
        /// Handles behavior for when update-related user settings have changed.
        /// </summary>
        /// <param name="sender">Source of the event - should be CopyFlyouts.Core.Settings (unused).</param>
        /// <param name="e">Property changed event arguments - used to see which property has changed.</param>
        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsManager.About.UpdateVersion):
                    if (_userAboutSettings.UpdateVersion is not null) { StopUpdateCheckTimer(); }
                    else { InitializeUpdateCheckTimer(); }
                    break;

                case nameof(SettingsManager.About.AutoUpdate):
                    if (_userAboutSettings.AutoUpdate) { InitializeUpdateCheckTimer(); }
                    else { StopUpdateCheckTimer();  }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Getter for latest release information, to compare against the current version.
        /// </summary>
        /// <returns>
        /// Task resulting in object representing the release information.
        /// May be null, in which case the information download has failed.
        /// </returns>
        private async Task<GitHubRelease?> GetLatestReleaseAsync()
        {
            try
            {
                var url = $"https://api.github.com/repos/krznck/copy-flyouts/releases/latest";
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _personalAccessToken);
                var response = await _client.GetStringAsync(url);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response, _jsonOptions);
                return release;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching release info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Behavior for when the user manually checks for an update.
        /// </summary>
        /// <remarks>
        /// The main reason to differenciate this from <see cref="CheckForUpdatesAutomatically"/>
        /// is that this is an action actively perpetrated by the user, and can be more obstructive, allowing us to use a MessageBox.
        /// </remarks>
        /// <returns>This is an asynchronous operation that returns a Task.</returns>
        public async Task CheckForUpdatesManually()
        {
            var latestRelease = await GetLatestReleaseAsync();

            if (latestRelease is null || _currentVersion is null || latestRelease.TagName is null)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new()
                {
                    Title = "Error",
                    Content = "Failed to check for updates."
                };
                await messageBox.ShowDialogAsync();
                return;
            }

            if (IsNewVersionAvailable(_currentVersion, latestRelease.TagName))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new ()
                {
                    Title = "New Updates",
                    Content = $"A new version - {latestRelease.TagName} - is available on GitHub!",
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonText = "Open update page"
                };

                _userAboutSettings.UpdateVersion = GetVersionFromString(latestRelease.TagName);

                if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary) { OpenUpdatePage(); }
            }
            else
            {
                Wpf.Ui.Controls.MessageBox messageBox = new ()
                {
                    Title = "No Updates",
                    Content = "You are using the latest version."
                };
                await messageBox.ShowDialogAsync();
            }
        }

        /// <summary>
        /// Behavior for when the program automatically checks for an update.
        /// </summary>
        /// <remarks>
        /// The main reason to differenciate this from <see cref="CheckForUpdatesManually"/>
        /// is that this is action -is not- actively perpetrated by the user, 
        /// and should not be obstructive, facilitating the use of a notification toast.
        /// </remarks>
        /// <returns>This is an asynchronous operation that returns a Task.</returns>
        private async Task CheckForUpdatesAutomatically()
        {
            var latestRelease = await GetLatestReleaseAsync();

            if (latestRelease is null || _currentVersion is null || latestRelease.TagName is null)
            {
                return;
            }

            if (IsNewVersionAvailable(_currentVersion, latestRelease.TagName))
            {
                _userAboutSettings.UpdateVersion = GetVersionFromString(latestRelease.TagName);
                NotifyAboutUpdate();
            }
        }

        /// <summary>
        /// Sends a notification toast that a new update is available, with an option to open its page.
        /// </summary>
        /// <remarks>
        /// Does not actually use the url taken from <see cref="GitHubRelease"/> and stored in the user <see cref="SettingsManager"/> objects,
        /// since we can just point to the latest releases and ensure the user always opens the newest version when clicking the button.
        /// </remarks>
        public static void NotifyAboutUpdate()
        {
            new ToastContentBuilder()
                .AddText("New update available")
                .AddText($"A new version is available!")
                .AddButton(new ToastButton()
                    .SetContent("Open update page")
                    .SetProtocolActivation(new Uri("https://github.com/krznck/copy-flyouts/releases/latest")))
                .Show();
        }

        /// <summary>
        /// Opens the latest release on GitHub in the user's browser.
        /// </summary>
        public void OpenUpdatePage()
        {
            if (_userAboutSettings.UpdateVersion is not null)
            {
                Process.Start(new ProcessStartInfo("https://github.com/krznck/copy-flyouts/releases/latest") { UseShellExecute = true });
            }
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            Version current = new (currentVersion);
            Version latest = GetVersionFromString(latestVersion);
            return latest.CompareTo(current) > 0;
        }

        private static Version GetVersionFromString(string version)
        {
            if (version.StartsWith('v'))
            {
                version = version[1..];
            }
            return new Version(version);
        }

        /// <summary>
        /// Starts the timer to automatically check for updates, based on the user's <see cref="SettingsManager"/> object.
        /// See <see cref="UpdateCheckTimer_Tick(object?, EventArgs)"/>.
        /// </summary>
        private void InitializeUpdateCheckTimer()
        {
            if (_userAboutSettings.UpdateVersion is not null || !_userAboutSettings.AutoUpdate)
            {
                return;
            }

            var time = UpdateFrequencies.Find(_userAboutSettings.UpdateFrequency);
            if (time is not null)
            {
                _updateCheckTimer.Interval = TimeSpan.FromMinutes(time.Value);
                _updateCheckTimer.Tick += UpdateCheckTimer_Tick;
                _updateCheckTimer.Start();
            }
        }

        private void StopUpdateCheckTimer()
        {
            _updateCheckTimer.Stop();
        }

        private async void UpdateCheckTimer_Tick(object? sender, EventArgs e)
        {
            await CheckForUpdatesAutomatically();
        }
    }
}
