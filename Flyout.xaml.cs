using System.Windows;

public partial class Flyout
{
    public Flyout(string textToShow)
    {
        InitializeComponent();
        this.Loaded += Flyout_Loaded;
        text.Text = textToShow;
    }

    private void Flyout_Loaded(object sender, RoutedEventArgs e)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var windowWidth = this.ActualWidth;
        var windowHeight = this.ActualHeight;

        this.Left = (screenWidth / 2) - (windowWidth / 2);
        this.Top = (screenHeight / 1.2) - (windowHeight / 2);
    }
}
