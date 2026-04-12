namespace RomanThurianApp;

public partial class AppShell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        // Force la resolution DI de la page Pokedex meme si Shell utilise un DataTemplate XAML.
        PokedexShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<PokedexPage>());
        TrainerShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<TrainerPage>());
    }
}