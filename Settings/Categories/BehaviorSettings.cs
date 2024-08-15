namespace CopyFlyouts.Settings.Categories
{
    using CopyFlyouts.Resources;

    /// <summary>
    /// Responsible for the settings for the behavior of the flyouts.
    /// </summary>
    public class BehaviorSettings : SettingHolder
    {
        private bool _enableKeyboardFlyouts = true;
        private bool _enableNonKeyboardFlyouts = true;
        private bool _allowImages = true;

        private double _flyoutLifetime = 1.5;
        private bool _enableFlyoutAnimations = true;

        private bool _flyoutUnderCursor = false;
        private string _flyoutScreen = "Follow cursor";
        private string _flyoutHorizontalAllignment = HorizontalScreenAllignments.Center.Name;
        private string _flyoutVerticalAllignment = VerticalScreenAllignments.BottomCenter.Name;

        private bool _enableSuccessSound = false;
        private string _chosenSuccessSound = SuccessSounds.Beep.Name;
        private bool _enableErrorSound = true;
        private string _chosenErrorSound = FailureSounds.Damage.Name;

        #region Public Properties

        public bool EnableKeyboardFlyouts
        {
            get => _enableKeyboardFlyouts;
            set
            {
                _enableKeyboardFlyouts = value;
                OnPropertyChanged(nameof(EnableKeyboardFlyouts));
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

        public bool AllowImages
        {
            get => _allowImages;
            set
            {
                _allowImages = value;
                OnPropertyChanged(nameof(AllowImages));
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

        public bool EnableFlyoutAnimations
        {
            get => _enableFlyoutAnimations;
            set
            {
                _enableFlyoutAnimations = value;
                OnPropertyChanged(nameof(EnableFlyoutAnimations));
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
                    int number = int.Parse(value[^1..]) - 1;
                    if (number < 0 || number > Screen.AllScreens.Length - 1)
                    {
                        _flyoutScreen = "Screen 1";
                    }
                    else
                    {
                        _flyoutScreen = value;
                    }
                }
                catch (FormatException) // however, if the user wrote anything that doesn't end in a number
                {
                    _flyoutScreen = "Follow cursor"; // then it should just be follow cursor
                }

                OnPropertyChanged(nameof(FlyoutScreen));
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

        #endregion

        public BehaviorSettings() { }
    }
}
