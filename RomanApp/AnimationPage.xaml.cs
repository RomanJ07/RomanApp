namespace RomanApp;

public partial class AnimationPage : ContentPage
{
    private Button? returnButton;

    public AnimationPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Récupère le bouton de retour
        returnButton = FindByName("ReturnButton") as Button;
        
        if (returnButton != null)
        {
            // Désactive le bouton au début (grisé)
            returnButton.IsEnabled = false;
            returnButton.BackgroundColor = Colors.LightGray;
            returnButton.TextColor = Colors.DarkGray;
        }

        // Attend que le GIF finisse de jouer (environ 3 secondes)
        await Task.Delay(3000);

        // Active le bouton après que le GIF ait joué une fois (jaune)
        if (returnButton != null)
        {
            returnButton.IsEnabled = true;
            returnButton.BackgroundColor = Color.FromArgb("#FFD700");
            returnButton.TextColor = Colors.Black;
        }
    }

    private async void OnReturnHomeClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}


