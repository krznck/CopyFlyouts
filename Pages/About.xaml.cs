namespace CopyFlyouts.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using CopyFlyouts.Resources;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;
    using CopyFlyouts.UpdateInfrastructure;

    /// <summary>
    /// Interaction logic for About.xaml.
    /// Handles informing the user about the program version, giving website link(s?),
    /// and acts as a setting page for the updates.
    /// </summary>
    public partial class About : Page
    {
        public UpdateChecker? UpdateChecker; // note: should be be passed by the MainWindow, so that they use the same instance
        private AboutSettings? _userAboutSettings;

        /// <summary>
        /// Initializes the <see cref="About"/> instance.
        /// Fills the one present combo box with data here,
        /// as that does not require anything derived from non-static objects.
        /// </summary>
        public About()
        {
            InitializeComponent();
            Loaded += About_Loaded;

            var frequencies = new List<string>();
            foreach (NamedValue frequency in UpdateFrequencies.Frequencies)
            {
                frequencies.Add(frequency.Name);
            }
            UpdateCheckFrequencyComboBox.ItemsSource = frequencies;
        }

        /// <summary>
        /// Event handler for the Loaded event.
        /// Importantly, this is where we get the user's Settings object from,
        /// as the settings are given to each page as DataContext.
        /// </summary>
        /// <param name="sender">Sender of the event, the <see cref="About"/> object itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SettingsManager settings)
            {
                return;
            }

            _userAboutSettings = settings.About;
            _userAboutSettings.PropertyChanged += UserSettings_PropertyChanged;

            LoadToggleLabels();
            RefreshUpdateStatusIndicators();
        }

        /// <summary>
        /// Handles the <see cref="SettingsManager"/> PropertyChanged event.
        /// Notably, it enables the update button if an update becomes available.
        /// </summary>
        /// <param name="sender">Sender of the event - <see cref="SettingsManager"/> object (unused).</param>
        /// <param name="e">Property changed event arguments - used to see which property has changed.</param>
        private void UserSettings_PropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (_userAboutSettings is null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(SettingsManager.About.UpdateVersion):
                    RefreshUpdateStatusIndicators();
                    break;

                case nameof(SettingsManager.About.AutoUpdate):
                    AutoUpdateLabel.Text = _userAboutSettings.AutoUpdate ? "On" : "Off";
                    break;

                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            if (_userAboutSettings is null)
            {
                return;
            }

            AutoUpdateLabel.Text = _userAboutSettings.AutoUpdate ? "On" : "Off";
        }

        private void RefreshUpdateStatusIndicators()
        {
            if (_userAboutSettings is null || _userAboutSettings.UpdateVersion is null)
            {
                OpenUpdatePageButton.IsEnabled = false;
                return;
            }

            OpenUpdatePageButton.IsEnabled = true;
            VersionSecondaryText.Visibility = Visibility.Visible;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_userAboutSettings is null)
            {
                return;
            }

            _userAboutSettings.Reset();
        }

        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateChecker is not null)
            {
                await UpdateChecker.CheckForUpdatesManually();
            }
        }

        private void OpenUpdatePageButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateChecker?.OpenUpdatePage();
        }
    }
}
