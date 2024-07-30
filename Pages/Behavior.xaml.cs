using copy_flyouts.Core;
using copy_flyouts.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
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
    /// Interaction logic for Behavior.xaml
    /// </summary>
    public partial class Behavior : Page
    {
        private Settings? _userSettings;

        public Behavior()
        {
            InitializeComponent();
            Loaded += Behavior_Loaded;

            var soundNames = new List<string>();
            foreach (Sound sound in FailureSounds.Sounds)
            {
                soundNames.Add(sound.Name);
            }
            SoundComboBox.ItemsSource = soundNames;

            var screenOptions = new List<string>();
            screenOptions.Add("Follow cursor");
            for (int i = 0; i <= System.Windows.Forms.Screen.AllScreens.Length - 1; i++)
            {
                string monitor = "Screen " + (i + 1).ToString();
                screenOptions.Add(monitor);
            }
            ScreenComboBox.ItemsSource = screenOptions;

            var successSoundNames = new List<string>();
            foreach (Sound sound in SuccessSounds.Sounds)
            {
                successSoundNames.Add(sound.Name);
            }
            SuccessSoundComboBox.ItemsSource = successSoundNames;

            var horizontalOptions = new List<string>();
            foreach (Allignment allignment in HorizontalScreenAllignments.Allignments)
            {
                horizontalOptions.Add(allignment.Name);
            }
            HorizontalAllignmentComboBox.ItemsSource = horizontalOptions;

            var verticalOptions = new List<string>();
            foreach (Allignment allignment in VerticalScreenAllignments.Allignments)
            {
                verticalOptions.Add(allignment.Name);
            }
            VerticalAllignmentComboBox.ItemsSource = verticalOptions;
        }

        private void Behavior_Loaded(object sender, RoutedEventArgs e)
        {
            _userSettings = DataContext as Settings;
            _userSettings.PropertyChanged += _userSettings_PropertyChanged;

            LoadToggleLabels();
        }

        private void _userSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.EnableKeyboardFlyouts):
                    EnableKeyboardFlyoutsLabel.Text = _userSettings.EnableKeyboardFlyouts ? "On" : "Off";
                    break;
                case nameof(Settings.EnableNonKeyboardFlyouts):
                    EnableNonKeyboardFlyoutsLabel.Text = _userSettings.EnableNonKeyboardFlyouts ? "On" : "Off";
                    break;
                case nameof(Settings.AllowImages):
                    AllowImagesLabel.Text = _userSettings.AllowImages ? "On" : "Off";
                    break;
                case nameof(Settings.EnableFlyoutAnimations):
                    FlyoutAnimationsLabel.Text = _userSettings.EnableFlyoutAnimations ? "On" : "Off";
                    break;
                case nameof(Settings.EnableSuccessSound):
                    EnableSuccessSoundsLabel.Text = _userSettings.EnableSuccessSound ? "On" : "Off";
                    break;
                case nameof(Settings.EnableErrorSound):
                    EnableErrorSoundsLabel.Text = _userSettings.EnableErrorSound ? "On" : "Off";
                    break;
                default:
                    break;
            }
        }

        private void LoadToggleLabels()
        {
            EnableKeyboardFlyoutsLabel.Text = _userSettings.EnableKeyboardFlyouts ? "On" : "Off";
            EnableNonKeyboardFlyoutsLabel.Text = _userSettings.EnableNonKeyboardFlyouts ? "On" : "Off";
            AllowImagesLabel.Text = _userSettings.AllowImages ? "On" : "Off";
            FlyoutAnimationsLabel.Text = _userSettings.EnableFlyoutAnimations ? "On" : "Off";
            EnableSuccessSoundsLabel.Text = _userSettings.EnableSuccessSound ? "On" : "Off";
            EnableErrorSoundsLabel.Text = _userSettings.EnableErrorSound ? "On" : "Off";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Core.Settings;

            if (userSettings != null)
            {
                // to be honest, I couldn't easily think of a good way to get the default values of those attributes somehow
                // so screw it, just hardcode them in
                userSettings.EnableKeyboardFlyouts = true;
                userSettings.EnableNonKeyboardFlyouts = true;
                userSettings.AllowImages = true;

                userSettings.FlyoutLifetime = 1.5;
                userSettings.EnableFlyoutAnimations = true;

                userSettings.FlyoutScreen = "Follow cursor";
                userSettings.FlyoutHorizontalAllignment = HorizontalScreenAllignments.Center.Name;
                userSettings.FlyoutVerticalAllignment = VerticalScreenAllignments.BottomCenter.Name;

                userSettings.EnableSuccessSound = false;
                userSettings.ChosenSuccessSound = SuccessSounds.Beep.Name;
                userSettings.EnableErrorSound = true;
                userSettings.ChosenErrorSound = FailureSounds.Damage.Name;
            }
        }

        private void FailurePlayButton_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Settings;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = FailureSounds.Find(userSettings.ChosenErrorSound).ResourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();
                    player.Play();
                }
            }
        }

        private void SuccessPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Settings;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = SuccessSounds.Find(userSettings.ChosenSuccessSound).ResourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();
                    player.Play();
                }
            }
        }
    }
}
