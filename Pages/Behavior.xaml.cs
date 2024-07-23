using copy_flyouts.Core;
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

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = DataContext as Core.Settings;

            if (userSettings != null)
            {
                // to be honest, I couldn't easily think of a good way to get the default values of those attributes somehow
                // so screw it, just hardcode them in
                userSettings.FlyoutLifetime = 1.5;
                userSettings.AllowImages = true;
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
    }
}
