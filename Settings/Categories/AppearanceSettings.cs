namespace CopyFlyouts.Settings.Categories
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Responsible for the settings for the appearance of flyouts and the program.
    /// </summary>
    public class AppearanceSettings : SettingHolder
    {
        private string _theme = "System";
        private bool _invertedTheme = false;
        private double _flyoutOpacity = 1.0;
        private double _flyoutWidthScale = 1.0;
        private double _flyoutWidth = 600;
        private double _flyoutHeightScale = 1.0;
        private double _flyoutHeight = 180;
        private double _flyoutFontSizeScale = 1.0;
        private double _flyoutFontSize = 20;
        private double _flyoutIconSize = 26;
        private int _flyoutCorners = 5;

        #region Public Properties

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

        #endregion

        public AppearanceSettings() { }
    }
}
