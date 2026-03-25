namespace RomanApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var services = IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("Services not initialized");
        var appShell = services.GetRequiredService<AppShell>();
        return new Window(appShell);
    }
}