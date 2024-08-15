namespace CopyFlyouts.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Interaction logic for General.xaml.
    /// Settings page for the general behavior of the program itself, rather than flyouts.
    /// </summary>
    public partial class General : Page
    {
        private GeneralSettings? _userGeneralSettings;

        /// <summary>
        /// Initalizes an instance of <see cref="General"/>.
        /// </summary>
        /// <remarks>
        /// Nothing special here - the important stuff happens in the <see cref="General_Loaded(object, RoutedEventArgs)"/> event handler.
        /// </remarks>
        public General()
        {
            InitializeComponent();
            Loaded += General_Loaded;
        }

        /// <summary>
        /// Event handler for the Loaded event.
        /// Importantly, this is where we get the user's Settings object from,
        /// as the settings are given to each page as DataContext.
        /// </summary>
        /// <param name="sender">Sender of the event, the <see cref="General"/> object itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void General_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SettingsManager settings)
            {
                return;
            }

            _userGeneralSettings = settings.General;
            _userGeneralSettings.PropertyChanged += UserSettings_PropertyChanged;

            LoadToggleLabels();
        }

        /// <summary>
        /// Handles the <see cref="SettingsManager"/> PropertyChanged event.
        /// Here, it is used to just switch between the On/Off toggle indicators.
        /// </summary>
        /// <remarks>
        /// Note: <see cref="Wpf.Ui.Controls.ToggleSwitch"/> objects actually have a way to do this natively within XAML,
        /// but... then they're on the right, and not on the left like it's on Windows.
        /// </remarks>
        /// <param name="sender">Sender of the event - <see cref="SettingsManager"/> object (unused).</param>
        /// <param name="e">Property changed event arguments - used to see which property has changed.</param>
        private void UserSettings_PropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (_userGeneralSettings is null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(SettingsManager.General.FlyoutsEnabled):
                    EnableCopyFlyoutsLabel.Text = _userGeneralSettings.FlyoutsEnabled
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.General.RunOnStartup):
                    RunOnStartupLabel.Text = _userGeneralSettings.RunOnStartup ? "On" : "Off";
                    break;

                case nameof(SettingsManager.General.StartMinimized):
                    StartMinimizedLabel.Text = _userGeneralSettings.StartMinimized ? "On" : "Off";
                    break;

                case nameof(SettingsManager.General.MinimizeToTray):
                    MinimizeToTrayLabel.Text = _userGeneralSettings.MinimizeToTray ? "On" : "Off";
                    // we're doing these text color changes as a way of visually disabling the text along with the CardControl itself,
                    // as the CardControl's IsEnabled does not change the appearance of those
                    NotifyMinimizedLabel.Appearance = _userGeneralSettings.MinimizeToTray
                        ? Wpf.Ui.Controls.TextColor.Primary
                        : Wpf.Ui.Controls.TextColor.Secondary;
                    MinimizeOnClosureLabel.Appearance = _userGeneralSettings.MinimizeToTray
                        ? Wpf.Ui.Controls.TextColor.Primary
                        : Wpf.Ui.Controls.TextColor.Secondary;
                    break;

                case nameof(SettingsManager.General.NotifyAboutMinimization):
                    NotifyMinimizedLabel.Text = _userGeneralSettings.NotifyAboutMinimization
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.General.MinimizeOnClosure):
                    MinimizeOnClosureLabel.Text = _userGeneralSettings.MinimizeOnClosure
                        ? "On"
                        : "Off";
                    break;

                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            if (_userGeneralSettings is null)
            {
                return;
            }

            EnableCopyFlyoutsLabel.Text = _userGeneralSettings.FlyoutsEnabled ? "On" : "Off";
            RunOnStartupLabel.Text = _userGeneralSettings.RunOnStartup ? "On" : "Off";
            StartMinimizedLabel.Text = _userGeneralSettings.StartMinimized ? "On" : "Off";
            MinimizeToTrayLabel.Text = _userGeneralSettings.MinimizeToTray ? "On" : "Off";
            NotifyMinimizedLabel.Text = _userGeneralSettings.NotifyAboutMinimization ? "On" : "Off";
            NotifyMinimizedLabel.Appearance = _userGeneralSettings.MinimizeToTray
                ? Wpf.Ui.Controls.TextColor.Primary
                : Wpf.Ui.Controls.TextColor.Secondary;
            MinimizeOnClosureLabel.Text = _userGeneralSettings.MinimizeOnClosure ? "On" : "Off";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_userGeneralSettings is null)
            {
                return;
            }

            _userGeneralSettings.Reset();
        }
    }
}
