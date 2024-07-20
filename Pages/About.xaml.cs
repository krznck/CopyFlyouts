using copy_flyouts.UpdateInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Linq.Expressions;
using Wpf.Ui.Controls;
using System.Windows.Threading;

namespace copy_flyouts.Pages
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        private UpdateChecker updateChecker = new UpdateChecker();

        public About()
        {
            InitializeComponent();
            VersionLabel.Text = "Version " + updateChecker.currentVersion;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await updateChecker.CheckForUpdatesManually();
        }
    }
}
