using Microsoft.UI.Xaml;

namespace divoom.app;

public partial class App : Application
{
    public App() => InitializeComponent();

    internal static MainWindow MainWindow { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}