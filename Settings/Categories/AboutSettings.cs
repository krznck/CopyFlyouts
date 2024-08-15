namespace CopyFlyouts.Settings.Categories
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using CopyFlyouts.Resources;

    /// <summary>
    /// Responsible for the settings related to the program version and updates.
    /// </summary>
    public class AboutSettings : SettingHolder
    {
        private Version? _updateVersion = null;
        private bool _autoUpdate = true;
        private string _updateFrequency = UpdateFrequencies.TwoHours.Name;

        #region PublicProperties

        /// <summary>
        /// Stores the update version of the program, to check whether the program can be updates.
        /// The special setter behavior is to ensure that when the store version becomes lower than
        /// the program version (which can happen when the program is updated), this is put back to null.
        /// </summary>
        public Version? UpdateVersion
        {
            get => _updateVersion;
            set
            {
                var commonResources = new ResourceDictionary
                {
                    Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative)
                };
                var currentVersionString = commonResources["Version"] as string;
                currentVersionString = currentVersionString?[1..];

                _ =
                    currentVersionString
                    ?? throw new NullReferenceException(
                        "Can't find current version resource string."
                    );

                var currentVersion = new Version(currentVersionString);

                if (value is not null && value.CompareTo(currentVersion) > 0)
                {
                    _updateVersion = value;
                }
                else
                {
                    _updateVersion = null;
                }
                OnPropertyChanged(nameof(UpdateVersion));
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
        /// The method is overriden from <see cref="SettingHolder.Reset()"/>
        /// to ensure <see cref="UpdateVersion"/> cannot be reset, as that is not shown to the user.
        /// </summary>
        public override void Reset()
        {
            var page = UpdateVersion;
            base.Reset();
            UpdateVersion = page;
        }
    }
}
