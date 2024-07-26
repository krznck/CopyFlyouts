﻿using copy_flyouts.Core;
using copy_flyouts.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace copy_flyouts.Pages
{
    /// <summary>
    /// Interaction logic for Behavior.xaml
    /// </summary>
    public partial class Behavior : Page
    {
        public Behavior()
        {
            InitializeComponent();

            var soundNames = new List<string>();
            foreach (Sound sound in FailureSounds.Sounds)
            {
                soundNames.Add(sound.Name);
            }
            SoundComboBox.ItemsSource = soundNames;

            var screenOptions = new List<string>();
            screenOptions.Add("Follow cursor");
            for (int i = 0; i <= System.Windows.Forms.Screen.AllScreens.Length - 1; i++)
            {
                string monitor = "Screen " + (i + 1).ToString();
                screenOptions.Add(monitor);
            }
            ScreenComboBox.ItemsSource = screenOptions;

            var successSoundNames = new List<string>();
            foreach (Sound sound in SuccessSounds.Sounds)
            {
                successSoundNames.Add(sound.Name);
            }
            SuccessSoundComboBox.ItemsSource = successSoundNames;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Core.Settings;

            if (userSettings != null)
            {
                // to be honest, I couldn't easily think of a good way to get the default values of those attributes somehow
                // so screw it, just hardcode them in
                userSettings.EnableKeyboardFlyouts = true;
                userSettings.EnableNonKeyboardFlyouts = true;
                userSettings.AllowImages = true;

                userSettings.FlyoutLifetime = 1.5;
                userSettings.EnableFlyoutAnimations = true;
                userSettings.FlyoutScreen = "Follow cursor";

                userSettings.EnableErrorSound = true;
                userSettings.ChosenErrorSound = "Damage";
            }
        }

        private void FailurePlayButton_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Settings;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = FailureSounds.Find(userSettings.ChosenErrorSound).ResourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();
                    player.Play();
                }
            }
        }

        private void SuccessPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Settings;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = SuccessSounds.Find(userSettings.ChosenSuccessSound).ResourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();
                    player.Play();
                }
            }
        }
    }
}
