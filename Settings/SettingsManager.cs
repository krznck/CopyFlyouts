namespace CopyFlyouts.Settings
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Windows;
    using System.Security;
    using CopyFlyouts.Settings.Categories;

    /// <summary>
    /// Holds different settings, and loads settings to and from a JSON file.
    /// </summary>
    public class SettingsManager : INotifyPropertyChanged
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public GeneralSettings General { get; set; } = new ();
        public BehaviorSettings Behavior { get; set; } = new ();
        public AppearanceSettings Appearance { get; set; } = new ();
        public AboutSettings About { get; set; } = new ();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Instantiates the <see cref="SettingsManager"/> object,
        /// and determines what the path of the JSON representation should be.
        /// </summary>
        public SettingsManager()
        {
            var appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = Application.Current.Resources["ProgramName"] as string ?? throw new ArgumentNullException("Cannot find program name.");

            // if we can write to the base directory, then we can just keep the settings there and let the program be portable,
            // but if not, then the program is probably installed in ProgramFiles for all users, and either way we cannot keep the settings there,
            // so we use AppData to have the settings be seperate for all users
            _filePath = IsWriteable(appBaseDirectory) ? Path.Combine(appBaseDirectory, "settings.json") : Path.Combine(appDataFolder, appName, "settings.json");
        }

        /// <summary>
        /// Copies the properties of one <see cref="SettingsManager"/> object to this instance.
        /// Used with a JSON deserializer to load data into the instance.
        /// </summary>
        /// <param name="settings"></param>
        private void CopySettings(SettingsManager settings)
        {
            if (settings is null) { return;  }

            General = settings.General;
            Behavior = settings.Behavior;
            Appearance = settings.Appearance;
            About = settings.About;
        }

        /// <summary>
        /// Attempts to load the JSON settings file into usable instance of <see cref="SettingsManager"/>,
        /// then copies that instance into this instance.
        /// </summary>
        /// <remarks>
        /// If the file has been corrupted in a way that isn't handled by the setters,
        /// it will reset the settings to default.
        /// </remarks>
        public void LoadSettingsFile()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var settings = JsonSerializer.Deserialize<SettingsManager>(json);
                    if (settings is not null) { CopySettings(settings); }
                }
                catch (JsonException)
                {
                    Debug.WriteLine("Invalid settings. Settings have been reset.");
                    SaveSettingsFile();
                }
            }
            else
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (directory is not null) { Directory.CreateDirectory(directory); }
            }

            General.PropertyChanged += Settings_PropertyChanged;
            Behavior.PropertyChanged += Settings_PropertyChanged;
            Appearance.PropertyChanged += Settings_PropertyChanged;
            About.PropertyChanged += Settings_PropertyChanged;
        }

        /// <summary>
        /// Intercepts the PropertyChanged event of a <see cref="SettingHolder"/> instance,
        /// and sends it forward to the subscribers of this <see cref="SettingsManager"/> instance.
        /// </summary>
        /// <remarks>
        /// This way, objects get notifications regardless of whether they subscribe to the general <see cref="SettingsManager"/>,
        /// or to specific <see cref="SettingHolder"/> instancse.
        /// </remarks>
        /// <param name="sender">Sender of the event, a <see cref="SettingHolder"/> instance (unused).</param>
        /// <param name="e">Property changed event arguments, used to get the name of the changed property.</param>
        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not null) { OnPropertyChanged(e.PropertyName); }
        }

        private void SaveSettingsFile()
        {
            var json = JsonSerializer.Serialize(this, jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        /// <summary>
        /// Sends changed properties to subscribers.
        /// Notably, also saves the settings to file, to ensure the changes are saved.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SaveSettingsFile();
        }

        /// <summary>
        /// Simple helper method that checks whether the program has permissions to write to a directory.
        /// Used to see whether we can make the settings portable.
        /// </summary>
        /// <param name="directoryPath">Path of the directory to be checked</param>
        /// <returns>Boolean representing whether we can write to the directory.</returns>
        private static bool IsWriteable(string directoryPath)
        {
            try
            {
                string tempFilePath = Path.Combine(directoryPath, Path.GetRandomFileName());

                // attempts to create and delete the temporary file
                using FileStream fs = File.Create(tempFilePath, 1, FileOptions.DeleteOnClose);

                return true;
            }
            catch (UnauthorizedAccessException) // no permissions
            {
                return false;
            }
            catch (SecurityException) // security policy disallows access
            {
                return false;
            }
            catch (DirectoryNotFoundException) // directory doesn't exist
            {
                return false;
            }
            catch (IOException) // other I.O (like directory is read-only)
            {
                return false;
            }
        }
    }
}
