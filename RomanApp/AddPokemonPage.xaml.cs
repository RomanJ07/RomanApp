namespace RomanApp;

public partial class AddPokemonPage : ContentPage
{
    public AddPokemonPage()
    {
        InitializeComponent();
        
        try
        {
            // Cree le ViewModel avec injection de service
            var services = IPlatformApplication.Current?.Services;
            if (services == null)
            {
                throw new InvalidOperationException("Services not initialized");
            }
            
            var capturedPokemonService = services.GetRequiredService<Services.ICapturedPokemonService>();
            var viewModel = new ViewModels.AddPokemonViewModel(capturedPokemonService);
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AddPokemonPage: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}
