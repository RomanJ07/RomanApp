namespace RomanApp;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnSeeActionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AnimationPage());
    }
}
