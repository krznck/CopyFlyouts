using copy_flyouts.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace copy_flyouts.Core
{
    public class Settings : INotifyPropertyChanged
    {
        private readonly string _filePath;
        private bool _flyoutsEnabled = true;
        private bool _startMinimized = false;
        private bool _minimizeToTray = true;
        private double _flyoutOpacity = 1.0;
        private double _flyoutWidthScale = 1.0;
        private double _flyoutWidth = 600;
        private double _flyoutHeightScale = 1.0;
        private double _flyoutHeight = 180;
        private double _flyoutFontSizeScale = 1.0;
        private double _flyoutFontSize = 20;
        private double _flyoutIconSize = 26;
        private string _theme = "System";
        private bool _invertedTheme = false;
        private bool _runOnStartup = false;
        private string? _updatePageUrl = null;
        private bool _autoUpdate = true;
        private double _flyoutLifetime = 1.5;
        private int _flyoutCorners = 5;
        private bool _allowImages = true;
        private bool _enableErrorSound = true;
        private bool _notifyAboutMinimization = true;
        private string _chosenErrorSound = FailureSounds.Damage.Name;
        private bool _enableNonKeyboardFlyouts = true;
        private bool _enableKeyboardFlyouts = true;
        private bool _enableFlyoutAnimations = true;
        private string _flyoutScreen = "Follow cursor";
        private bool _enableSuccessSound = false;
        private string _chosenSuccessSound = SuccessSounds.Beep.Name;
        private string _flyoutHorizontalAllignment = HorizontalScreenAllignments.Center.Name;
        private string _flyoutVerticalAllignment = VerticalScreenAllignments.BottomCenter.Name;
        private bool _flyoutUnderCursor = false;
        public event PropertyChangedEventHandler? PropertyChanged;

        #region PublicProperties
        public bool FlyoutsEnabled
        {
            get => _flyoutsEnabled;
            set
            {
                _flyoutsEnabled = value;
                OnPropertyChanged(nameof(FlyoutsEnabled));
            }
        }

        public bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                _startMinimized = value;
                OnPropertyChanged(nameof(StartMinimized));
            }
        }

        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                _minimizeToTray = value;
                OnPropertyChanged(nameof(MinimizeToTray));
            }
        }

        public double FlyoutOpacity
        {
            get => _flyoutOpacity;
            set
            {
                _flyoutOpacity = RestrictDouble(0.3, 1, RoundToNearestTenth(value));
                OnPropertyChanged(nameof(FlyoutOpacity));
            }
        }

        public double FlyoutWidthScale
        {
            get => _flyoutWidthScale;
            set
            {
                _flyoutWidthScale = RestrictDouble(0.5, 3.0, RoundToNearestTenth(value));
                FlyoutWidth = 600 * _flyoutWidthScale;
                OnPropertyChanged(nameof(FlyoutWidthScale));
            }
        }

        [JsonIgnore]
        public double FlyoutWidth
        {
            get => _flyoutWidth;
            set
            {
                _flyoutWidth = value;
                OnPropertyChanged(nameof(FlyoutWidth));
            }
        }

        public double FlyoutHeightScale
        {
            get => _flyoutHeightScale;
            set
            {
                _flyoutHeightScale = RestrictDouble(0.5, 3.0, RoundToNearestTenth(value));
                FlyoutHeight = 180 * _flyoutHeightScale;
                OnPropertyChanged(nameof(FlyoutHeightScale));
            }
        }

        [JsonIgnore]
        public double FlyoutHeight
        {
            get => _flyoutHeight;
            set
            {
                _flyoutHeight = value;
                OnPropertyChanged(nameof(FlyoutHeight));
            }
        }

        public double FlyoutFontSizeScale
        {
            get => _flyoutFontSizeScale;
            set
            {
                _flyoutFontSizeScale = RestrictDouble(0.5, 3.0, RoundToNearestTenth(value));
                FlyoutFontSize = 20 * _flyoutFontSizeScale;
                FlyoutIconSize = 26 * _flyoutFontSizeScale;
                OnPropertyChanged(nameof(FlyoutFontSizeScale));
            }
        }

        [JsonIgnore]
        public double FlyoutFontSize
        {
            get => _flyoutFontSize;
            set
            {
                _flyoutFontSize = value;
                OnPropertyChanged(nameof(FlyoutFontSize));
            }
        }

        [JsonIgnore]
        public double FlyoutIconSize
        {
            get => _flyoutIconSize;
            set
            {
                _flyoutIconSize = value;
                OnPropertyChanged(nameof(FlyoutIconSize));
            }
        }

        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        public bool InvertedTheme
        {
            get => _invertedTheme;
            set
            {
                _invertedTheme = value;
                OnPropertyChanged(nameof(InvertedTheme));
            }
        }

        public bool RunOnStartup
        {
            get => _runOnStartup;
            set
            {
                _runOnStartup = value;
                OnPropertyChanged(nameof(RunOnStartup));
            }
        }

        public string? UpdatePageUrl
        {
            get => _updatePageUrl;
            set
            {
                _updatePageUrl = value;
                OnPropertyChanged(nameof(UpdatePageUrl));
            }
        }

        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                _autoUpdate = value;    
                OnPropertyChanged(nameof(AutoUpdate));
            }
        }

        public double FlyoutLifetime
        {
            get => _flyoutLifetime;
            set
            {
                _flyoutLifetime = RestrictDouble(0.3, 5.0, RoundToNearestTenth(value));
                OnPropertyChanged(nameof(FlyoutLifetime));
            }
        }

        public int FlyoutCorners
        {
            get => _flyoutCorners;
            set
            {
                if (value < 0) value = 0;
                if (value > 20) value = 20;
                _flyoutCorners = value;
                OnPropertyChanged(nameof(FlyoutCorners));
            }
        }

        public bool AllowImages
        {
            get => _allowImages;
            set
            {
                _allowImages = value;
                OnPropertyChanged(nameof(AllowImages));
            }
        }

        public bool EnableErrorSound
        {
            get => _enableErrorSound;
            set
            {
                _enableErrorSound = value;
                OnPropertyChanged(nameof(EnableErrorSound));
            }
        }

        public string ChosenErrorSound
        {
            get => _chosenErrorSound;
            set
            {
                if (FailureSounds.Find(value) is null)
                {
                    _chosenErrorSound = FailureSounds.Damage.Name;
                }
                else
                {
                    _chosenErrorSound = value;
                }
                OnPropertyChanged(nameof(ChosenErrorSound));
            }
        }

        public bool NotifyAboutMinimization
        {
            get => _notifyAboutMinimization;
            set
            {
                _notifyAboutMinimization = value;
                OnPropertyChanged(nameof(NotifyAboutMinimization));
            }
        }

        public bool EnableNonKeyboardFlyouts
        {
            get => _enableNonKeyboardFlyouts;
            set
            {
                _enableNonKeyboardFlyouts = value;
                OnPropertyChanged(nameof(EnableNonKeyboardFlyouts));
            }
        }

        public bool EnableKeyboardFlyouts
        {
            get => _enableKeyboardFlyouts;
            set
            {
                _enableKeyboardFlyouts = value;
                OnPropertyChanged(nameof(EnableKeyboardFlyouts));
            }
        }

        public bool EnableFlyoutAnimations
        {
            get => _enableFlyoutAnimations;
            set
            {
                _enableFlyoutAnimations = value;
                OnPropertyChanged(nameof(EnableFlyoutAnimations));
            }
        }

        public string FlyoutScreen
        {
            get => _flyoutScreen;
            set
            {
                try
                {
                    // if the user specified a screen, that screen will be chosen
                    // however, if the screen is outside of the range of available screen (i.e. no longer connected),
                    // then it will default to 1. This is here to reflect that in the settings and not leave the option blank
                    int number = int.Parse(value.Substring(value.Length - 1)) - 1;
                    if (number < 0 || number > Screen.AllScreens.Length - 1) { _flyoutScreen = "Screen 1";  }
                    else { _flyoutScreen = value; }
                }
                catch (FormatException) // however, if the user wrote anything that doesn't end in a number
                {
                    _flyoutScreen = "Follow cursor"; // then it should just be follow cursor
                }

                OnPropertyChanged(nameof(FlyoutScreen));
            }
        }

        public bool EnableSuccessSound
        {
            get => _enableSuccessSound;
            set
            {
                _enableSuccessSound = value;
                OnPropertyChanged(nameof(EnableSuccessSound));
            }
        }

        public string ChosenSuccessSound
        {
            get => _chosenSuccessSound;
            set
            {
                if (SuccessSounds.Find(value) is null)
                {
                    _chosenSuccessSound = SuccessSounds.Beep.Name;
                }
                else
                {
                    _chosenSuccessSound = value;
                }
                OnPropertyChanged(nameof(ChosenSuccessSound));
            }
        }

        public string FlyoutHorizontalAllignment
        {
            get => _flyoutHorizontalAllignment;
            set
            {
                if (HorizontalScreenAllignments.Find(value) is null)
                {
                    _flyoutHorizontalAllignment = HorizontalScreenAllignments.Center.Name;
                }
                else
                {
                    _flyoutHorizontalAllignment = value;
                }
                OnPropertyChanged(nameof(FlyoutHorizontalAllignment));
            }
        }

        public string FlyoutVerticalAllignment
        {
            get => _flyoutVerticalAllignment;
            set
            {
                if (VerticalScreenAllignments.Find(value) is null)
                {
                    _flyoutVerticalAllignment = VerticalScreenAllignments.BottomCenter.Name;
                }
                else
                {
                    _flyoutVerticalAllignment = value;
                }
                OnPropertyChanged(nameof(FlyoutVerticalAllignment));
            }
        }

        public bool FlyoutUnderCursor
        {
            get => _flyoutUnderCursor;
            set
            {
                _flyoutUnderCursor = value;
                OnPropertyChanged(nameof(FlyoutUnderCursor));
            }
        }

        #endregion

        private double RoundToNearestTenth(double number)
        {
            return Math.Round(number * 10, MidpointRounding.AwayFromZero) / 10;
        }

        private double RestrictDouble(double minimum, double maximum, double value)
        {
            if (value < minimum)
            {
                value = minimum;
            }
            else if (value > maximum)
            {
                value = maximum;
            }

            return value;
        }

        public Settings()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;
            _filePath = Path.Combine(appDataFolder, appName, "settings.json");

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            LoadSettingsFile();
        }

        [JsonConstructor]
        public Settings(
            bool flyoutsEnabled = true,
            bool startMinimized = false,
            bool minimizeToTray = true,
            double flyoutOpacity = 1.0,
            double flyoutWidthScale = 1.0,
            double flyoutHeightScale = 1.0,
            double flyoutFontSizeScale = 1.0,
            string theme = "System",
            bool invertedTheme = false,
            bool runOnStartup = false,
            string? updatePageUrl = null,
            bool autoUpdate = true,
            double flyoutLifetime = 1,
            int flyoutCorners = 5,
            bool allowImages = true,
            bool enableErrorSound = true,
            string? chosenErrorSound = null,
            bool notifyAboutMinimization = true,
            bool enableNonKeyboardFlyouts = true,
            bool enableKeyboardFlyouts = true,
            bool enableFlyoutAnimations = true,
            string flyoutScreen = "Follow cursor",
            bool enableSuccessSound = false,
            string? chosenSuccessSound = null,
            string? flyoutHorizontalAllignment = null,
            string? flyoutVerticalAllignment = null,
            bool flyoutUnderCursor = false)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = System.Windows.Application.Current.Resources["ProgramName"] as string;
            _filePath = Path.Combine(appDataFolder, appName, "settings.json");

            FlyoutsEnabled = flyoutsEnabled;
            StartMinimized = startMinimized;
            MinimizeToTray = minimizeToTray;
            FlyoutOpacity = flyoutOpacity;
            FlyoutWidthScale = flyoutWidthScale;
            FlyoutHeightScale = flyoutHeightScale;
            FlyoutFontSizeScale = flyoutFontSizeScale;
            Theme = theme;
            InvertedTheme = invertedTheme;
            RunOnStartup = runOnStartup;
            UpdatePageUrl = updatePageUrl;
            AutoUpdate = autoUpdate;
            FlyoutLifetime = flyoutLifetime;
            FlyoutCorners = flyoutCorners;
            AllowImages = allowImages;
            EnableErrorSound = enableErrorSound;

            if (chosenErrorSound is null) { chosenErrorSound = FailureSounds.Damage.Name; }
            ChosenErrorSound = chosenErrorSound;

            NotifyAboutMinimization = notifyAboutMinimization;
            EnableNonKeyboardFlyouts = enableNonKeyboardFlyouts;
            EnableKeyboardFlyouts = enableKeyboardFlyouts;
            EnableFlyoutAnimations = enableFlyoutAnimations;
            FlyoutScreen = flyoutScreen;
            EnableSuccessSound = enableSuccessSound;

            if (chosenSuccessSound is null) { chosenSuccessSound = SuccessSounds.Beep.Name; }
            ChosenSuccessSound = chosenSuccessSound;

            if (flyoutHorizontalAllignment is null) { flyoutHorizontalAllignment = HorizontalScreenAllignments.Center.Name; }
            FlyoutHorizontalAllignment = flyoutHorizontalAllignment;

            if (flyoutVerticalAllignment is null) { flyoutVerticalAllignment = VerticalScreenAllignments.BottomCenter.Name; };
            FlyoutVerticalAllignment = flyoutVerticalAllignment;

            FlyoutUnderCursor = flyoutUnderCursor;
        }

        private void CopySettings(Settings settings)
        {
            if (settings != null)
            {
                FlyoutsEnabled = settings.FlyoutsEnabled;
                StartMinimized = settings.StartMinimized;
                MinimizeToTray = settings.MinimizeToTray;
                FlyoutOpacity = settings.FlyoutOpacity;
                FlyoutWidthScale = settings.FlyoutWidthScale;
                FlyoutHeightScale = settings.FlyoutHeightScale;
                FlyoutFontSizeScale = settings.FlyoutFontSizeScale;
                Theme = settings.Theme;
                InvertedTheme = settings.InvertedTheme;
                RunOnStartup = settings.RunOnStartup;
                UpdatePageUrl = settings.UpdatePageUrl;
                AutoUpdate = settings.AutoUpdate;
                FlyoutLifetime = settings.FlyoutLifetime;
                FlyoutCorners = settings.FlyoutCorners;
                AllowImages = settings.AllowImages;
                EnableErrorSound = settings.EnableErrorSound;
                ChosenErrorSound = settings.ChosenErrorSound;
                NotifyAboutMinimization = settings.NotifyAboutMinimization;
                EnableNonKeyboardFlyouts = settings.EnableNonKeyboardFlyouts;
                EnableKeyboardFlyouts = settings.EnableKeyboardFlyouts;
                EnableFlyoutAnimations = settings.EnableFlyoutAnimations;
                FlyoutScreen = settings.FlyoutScreen;
                EnableSuccessSound = settings.EnableSuccessSound;
                ChosenSuccessSound = settings.ChosenSuccessSound;
                FlyoutHorizontalAllignment = settings.FlyoutHorizontalAllignment;
                FlyoutVerticalAllignment = settings.FlyoutVerticalAllignment;
                FlyoutUnderCursor = settings.FlyoutUnderCursor;

                // note: this little block checks that the saved upate url (which indicates that there is an update)
                // is not pointing to a program version that is lower than the current program.
                // this is to ensure that when the user UPDATES the program, it removes the update url as it is now outdates
                var commonResources = new ResourceDictionary();
                commonResources.Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative);
                string version = commonResources["Version"] as string;
                version = version.Substring(1);

                if (UpdatePageUrl != null)
                {
                    string pattern = @"\d+\.\d+\.\d+";
                    Match match = Regex.Match(UpdatePageUrl, pattern);
                    {
                        if (match.Success)
                        {
                            Version urlVersion = new Version(match.Value);
                            Version programVersion = new Version(version);
                            if (urlVersion.CompareTo(programVersion) < 0)
                            {
                                UpdatePageUrl = null;
                            }
                        }
                    }
                }
            }
        }

        private void LoadSettingsFile()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    CopySettings(JsonSerializer.Deserialize<Settings>(json));
                }
                catch (JsonException)
                {
                    Debug.WriteLine("Invalid settings. Settings have been reset.");
                    SaveSettingsFile();
                }
            }

        }

        private void SaveSettingsFile()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SaveSettingsFile();
        }
    }
}
