namespace CopyFlyouts.Settings.Categories
{
    /// <summary>
    /// Responsible for the settings for general behavior of the program itself, rather than flyouts.
    /// </summary>
    public class GeneralSettings : SettingHolder
    {
        private bool _flyoutsEnabled = true;
        private bool _runOnStartup = false;
        private bool _startMinimized = false;
        private bool _minimizeToTray = true;
        private bool _notifyAboutMinimization = true;
        private bool _minimizeOnClosure = false;

        #region Public Properties

        public bool FlyoutsEnabled
        {
            get => _flyoutsEnabled;
            set
            {
                _flyoutsEnabled = value;
                OnPropertyChanged(nameof(FlyoutsEnabled));
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

        public bool NotifyAboutMinimization
        {
            get => _notifyAboutMinimization;
            set
            {
                _notifyAboutMinimization = value;
                OnPropertyChanged(nameof(NotifyAboutMinimization));
            }
        }

        public bool MinimizeOnClosure
        {
            get => _minimizeOnClosure;
            set
            {
                _minimizeOnClosure = value;
                OnPropertyChanged(nameof(MinimizeOnClosure));
            }
        }

        #endregion

        public GeneralSettings() { }
    }
}
