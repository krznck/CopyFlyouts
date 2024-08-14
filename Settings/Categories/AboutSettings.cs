namespace CopyFlyouts.Settings.Categories
{
    using CopyFlyouts.Resources;
    using System.Text.RegularExpressions;
    using System.Windows;

    /// <summary>
    /// Responsible for the settings related to the program version and updates.
    /// </summary>
    public class AboutSettings : SettingHolder
    {
        private string? _updatePageUrl = null;
        private bool _autoUpdate = true;
        private string _updateFrequency = UpdateFrequencies.TwoHours.Name;

        #region PublicProperties

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

        public string UpdateFrequency
        {
            get => _updateFrequency;
            set
            {
                if (UpdateFrequencies.Find(value) is null)
                {
                    _updateFrequency = UpdateFrequencies.TwoHours.Name;
                }
                else
                {
                    _updateFrequency = value;
                }
                OnPropertyChanged(nameof(UpdateFrequency));
            }
        }

        #endregion

        /// <summary>
        /// Instantiates the <see cref="AboutSettings"/> object.
        /// </summary>
        /// <remarks>
        /// This constructor checks that the saved upate url (which indicates that there is an update)
        /// is not pointing to a program version that is lower than the current program.
        /// This is to ensure that when the user UPDATES the program, it removes the update url as it is now outdates.
        /// </remarks>
        public AboutSettings()
        {
            if (UpdatePageUrl is null) { return; }

            var commonResources = new ResourceDictionary
            {
                Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative)
            };
            string? version = commonResources["Version"] as string;
            version = version?[1..];

            string pattern = @"\d+\.\d+\.\d+";
            Match match = Regex.Match(UpdatePageUrl, pattern);
            if (match.Success && version is not null)
            {
                Version urlVersion = new(match.Value);
                Version programVersion = new(version);
                if (urlVersion.CompareTo(programVersion) < 0)
                {
                    UpdatePageUrl = null;
                }
            }
        }

        /// <summary>
        /// The method is overriden from <see cref="SettingHolder.Reset()"/> 
        /// to ensure <see cref="UpdatePageUrl"/> cannot be reset, as that is not shown to the user.
        /// </summary>
        public override void Reset()
        {
            var page = UpdatePageUrl;
            base.Reset();
            UpdatePageUrl = page;
        }
    }
}
