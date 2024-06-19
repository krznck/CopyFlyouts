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
        private double _flyoutOpacity = 1.0;
        private double _flyoutWidthScale = 1.0;
        private double _flyoutWidth = 600;
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
                _flyoutOpacity = Math.Truncate(value * 100) / 100; // truncates ridiculous values like 0.50000000000002 into 0.5
                OnPropertyChanged(nameof(FlyoutOpacity));
            }
        }

        public double FlyoutWidthScale
        {
            get => _flyoutWidthScale;
            set
            {
                _flyoutWidthScale = Math.Truncate(value * 100) / 100; // truncates ridiculous values like 0.50000000000002 into 0.5
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
        #endregion

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
        public Settings(bool flyoutsEnabled, bool startMinimized, bool minimizeToTray, double flyoutOpacity, double flyoutWidthScale)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var commonResources = new ResourceDictionary();
            commonResources.Source = new Uri("Resources/CommonResources.xaml", UriKind.Relative);
            var appName = commonResources["ProgramName"] as string;
            _filePath = Path.Combine(appDataFolder, appName, "settings.json");

            FlyoutsEnabled = flyoutsEnabled;
            StartMinimized = startMinimized;
            MinimizeToTray = minimizeToTray;
            FlyoutOpacity = flyoutOpacity;
            FlyoutWidthScale = flyoutWidthScale;
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
