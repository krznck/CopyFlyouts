﻿namespace CopyFlyouts.Settings
{
    using System.ComponentModel;

    /// <summary>
    /// Abstract class representing a holders of settings,
    /// i.e. objects that contain configurable user settings that change the behavior of the program.
    /// </summary>
    public abstract class SettingHolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sends changed properties to subscribers.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper method to prevent double precision problems.
        /// </summary>
        /// <param name="number">Double to be rounded.</param>
        /// <returns>Double rounded to the nearest tenth.</returns>
        protected static double RoundToNearestTenth(double number)
        {
            return Math.Round(number * 10, MidpointRounding.AwayFromZero) / 10;
        }

        /// <summary>
        /// Helper method to ensure doubles can't be given inappropriate values.
        /// </summary>
        /// <param name="minimum">Minimum value that the double should take.</param>
        /// <param name="maximum">Maximum value that the double should take.</param>
        /// <param name="value">Actual value that the double has been given.</param>
        /// <returns>Double with value corrected to match upper and lower floor.</returns>
        protected static double RestrictDouble(double minimum, double maximum, double value)
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

        /// <summary>
        /// Public method that resets all of the setting attributes to their default values.
        /// </summary>
        /// <remarks>
        /// Can be overwritten in case we don't want some attribute to be reset.
        /// </remarks>
        public virtual void Reset()
        {
            var defaultInstance = Activator.CreateInstance(GetType());
            foreach (var property in GetType().GetProperties())
            {
                if (property.CanWrite)
                {
                    var defaultValue = property.GetValue(defaultInstance);
                    var currentValue = property.GetValue(this);

                    if (!Equals(currentValue, defaultValue))
                    {
                        property.SetValue(this, defaultValue);
                    }
                }
            }
        }
    }
}
