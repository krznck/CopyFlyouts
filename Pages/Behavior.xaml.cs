namespace CopyFlyouts.Pages
{
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using CopyFlyouts.Resources;
    using CopyFlyouts.Settings;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Interaction logic for Behavior.xaml.
    /// Settings page for the behavior of the flyouts.
    /// </summary>
    public partial class Behavior : Page
    {
        private BehaviorSettings? _userBehaviorSettings;

        /// <summary>
        /// Initializes the <see cref="Behavior"/> instance.
        /// Fills out all the different <see cref="ComboBox"/>es with their information,
        /// as they don't require anything that can't be static.
        /// </summary>
        public Behavior()
        {
            InitializeComponent();
            Loaded += Behavior_Loaded;

            FillComboBoxes();
        }

        /// <summary>
        /// Event handler for the Loaded event.
        /// Importantly, this is where we get the user's Settings object from,
        /// as the settings are given to each page as DataContext.
        /// </summary>
        /// <param name="sender">Sender of the event, the <see cref="Behavior"/> object itself (unused).</param>
        /// <param name="e">Routed event arguments (unused).</param>
        private void Behavior_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SettingsManager settings)
            {
                return;
            }

            _userBehaviorSettings = settings.Behavior;
            _userBehaviorSettings.PropertyChanged += UserSettings_PropertyChanged;

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
        /// <param name="e">Property changed event arguments (unused).</param>
        private void UserSettings_PropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (_userBehaviorSettings is null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(SettingsManager.Behavior.EnableKeyboardFlyouts):
                    EnableKeyboardFlyoutsLabel.Text = _userBehaviorSettings.EnableKeyboardFlyouts
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.Behavior.EnableNonKeyboardFlyouts):
                    EnableNonKeyboardFlyoutsLabel.Text =
                        _userBehaviorSettings.EnableNonKeyboardFlyouts ? "On" : "Off";
                    break;

                case nameof(SettingsManager.Behavior.AllowImages):
                    AllowImagesLabel.Text = _userBehaviorSettings.AllowImages ? "On" : "Off";
                    break;

                case nameof(SettingsManager.Behavior.EnableFlyoutAnimations):
                    FlyoutAnimationsLabel.Text = _userBehaviorSettings.EnableFlyoutAnimations
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.Behavior.EnableSuccessSound):
                    EnableSuccessSoundsLabel.Text = _userBehaviorSettings.EnableSuccessSound
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.Behavior.EnableErrorSound):
                    EnableErrorSoundsLabel.Text = _userBehaviorSettings.EnableErrorSound
                        ? "On"
                        : "Off";
                    break;

                case nameof(SettingsManager.Behavior.FlyoutUnderCursor):
                    FlyoutsUnderCursorLabel.Text = _userBehaviorSettings.FlyoutUnderCursor
                        ? "On"
                        : "Off";
                    break;

                default:
                    break;
            }
        }

        #region Click Event Handlers

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_userBehaviorSettings is null)
            {
                return;
            }

            _userBehaviorSettings.Reset();
        }

        private static void UpdateAllToggleButtonBindings(DependencyObject parent)
        {
            // Iterate through all children in the visual tree
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // If the child is a ToggleButton, update its binding
                if (child is ToggleButton toggleButton)
                {
                    var bindingExpression = toggleButton.GetBindingExpression(
                        ToggleButton.IsCheckedProperty
                    );
                    bindingExpression?.UpdateTarget();
                }

                // Recursively call this method for each child
                UpdateAllToggleButtonBindings(child);
            }
        }

        private void FailurePlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_userBehaviorSettings is null)
            {
                return;
            }

            var sound = FailureSounds.Find(_userBehaviorSettings.ChosenErrorSound);

            if (sound is null)
            {
                return;
            }

            string assetPath = sound.AssetPath;
            PlaySound(assetPath);
        }

        private void SuccessPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (_userBehaviorSettings is null)
            {
                return;
            }

            var sound = SuccessSounds.Find(_userBehaviorSettings.ChosenSuccessSound);

            if (sound is null)
            {
                return;
            }

            string assetPath = sound.AssetPath;
            PlaySound(assetPath);
        }

        #endregion

        private void LoadToggleLabels()
        {
            if (_userBehaviorSettings is null)
            {
                return;
            }

            EnableKeyboardFlyoutsLabel.Text = _userBehaviorSettings.EnableKeyboardFlyouts
                ? "On"
                : "Off";
            EnableNonKeyboardFlyoutsLabel.Text = _userBehaviorSettings.EnableNonKeyboardFlyouts
                ? "On"
                : "Off";
            AllowImagesLabel.Text = _userBehaviorSettings.AllowImages ? "On" : "Off";
            FlyoutAnimationsLabel.Text = _userBehaviorSettings.EnableFlyoutAnimations
                ? "On"
                : "Off";
            EnableSuccessSoundsLabel.Text = _userBehaviorSettings.EnableSuccessSound ? "On" : "Off";
            EnableErrorSoundsLabel.Text = _userBehaviorSettings.EnableErrorSound ? "On" : "Off";
            FlyoutsUnderCursorLabel.Text = _userBehaviorSettings.FlyoutUnderCursor ? "On" : "Off";
        }

        private static void PlaySound(string assetPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(assetPath);
            if (stream is not null)
            {
                SoundPlayer player = new(stream);
                player.Load();
                player.Play();
            }
        }

        private void FillComboBoxes()
        {
            // extracted this here because it really cluttered the constructor

            var screenOptions = new List<string> { "Follow cursor" };
            for (int i = 0; i <= Screen.AllScreens.Length - 1; i++)
            {
                string monitor = "Screen " + (i + 1).ToString();
                screenOptions.Add(monitor);
            }
            ScreenComboBox.ItemsSource = screenOptions;

            var horizontalOptions = new List<string>();
            foreach (NamedValue allignment in HorizontalScreenAllignments.Allignments)
            {
                horizontalOptions.Add(allignment.Name);
            }
            HorizontalAllignmentComboBox.ItemsSource = horizontalOptions;

            var verticalOptions = new List<string>();
            foreach (NamedValue allignment in VerticalScreenAllignments.Allignments)
            {
                verticalOptions.Add(allignment.Name);
            }
            VerticalAllignmentComboBox.ItemsSource = verticalOptions;

            var soundNames = new List<string>();
            foreach (NamedAssetPath sound in FailureSounds.Sounds)
            {
                soundNames.Add(sound.Name);
            }
            SoundComboBox.ItemsSource = soundNames;

            var successSoundNames = new List<string>();
            foreach (NamedAssetPath sound in SuccessSounds.Sounds)
            {
                successSoundNames.Add(sound.Name);
            }
            SuccessSoundComboBox.ItemsSource = successSoundNames;
        }
    }
}
