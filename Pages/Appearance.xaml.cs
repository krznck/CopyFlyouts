namespace CopyFlyouts.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Interaction logic for Appearance.xaml.
    /// Settings page for the appearance of the flyout,
    /// and to some degree the program in general.
    /// </summary>
    public partial class Appearance : Page
    {
        private AppearanceSettings? _userAppearanceSettings;

        /// <summary>
        /// Initializes the <see cref="Appearance"/> instance.
        /// Fills the one present combo box with data here, 
        /// as that does not require anything derived from other objects.
        /// </summary>
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

        /// <summary>
        /// Event handler for the Loaded event.
        /// Importantly, this is where we get the user's Settings object from,
        /// as the settings are given to each page as DataContext.
        /// </summary>
        /// <param name="sender">Sender of the event, the <see cref="Appearance"/> object itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void Appearance_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SettingsManager settings) { return; }

            _userAppearanceSettings = settings.Appearance;
            _userAppearanceSettings.PropertyChanged += UserSettings_PropertyChanged;

            LoadToggleLabels();
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_userAppearanceSettings is null) { return; }

            switch (e.PropertyName)
            {
                case nameof(SettingsManager.Appearance.InvertedTheme):
                    InvertedThemeLabel.Text = _userAppearanceSettings.InvertedTheme ? "On" : "Off";
                    break;

                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            if (_userAppearanceSettings is null) { return; }

            InvertedThemeLabel.Text = _userAppearanceSettings.InvertedTheme ? "On" : "Off";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_userAppearanceSettings is null) { return; }

            _userAppearanceSettings.Theme = "System";
            _userAppearanceSettings.InvertedTheme = false;

            _userAppearanceSettings.FlyoutOpacity = 1.0;
            _userAppearanceSettings.FlyoutWidthScale = 1.0;
            _userAppearanceSettings.FlyoutHeightScale = 1.0;
            _userAppearanceSettings.FlyoutFontSizeScale = 1.0;
            _userAppearanceSettings.FlyoutCorners = 5;
        }
    }
}
