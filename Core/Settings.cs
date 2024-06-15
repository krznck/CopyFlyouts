using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public Settings()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var commonResources = new ResourceDictionary();
            commonResources.Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative);
            var appName = commonResources["ProgramName"] as string;
            _filePath = Path.Combine(appDataFolder, appName, "settings.json");

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            LoadSettingsFile();
        }

        [JsonConstructor]
        public Settings(bool flyoutsEnabled, bool startMinimized, bool minimizeToTray)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var commonResources = new ResourceDictionary();
            commonResources.Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative);
            var appName = commonResources["ProgramName"] as string;
            _filePath = Path.Combine(appDataFolder, appName, "settings.json");

            _flyoutsEnabled = flyoutsEnabled;
            _startMinimized = startMinimized;
            _minimizeToTray = minimizeToTray;
        }

        private void CopySettings(Settings settings)
        {
            if (settings != null)
            {
                _flyoutsEnabled = settings.FlyoutsEnabled;
                _startMinimized = settings.StartMinimized;
                _minimizeToTray = settings.MinimizeToTray;
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
