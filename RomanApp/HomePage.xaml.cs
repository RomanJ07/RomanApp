namespace RomanApp;

public partial class HomePage
{
    private bool _is3dLoaded;

    public HomePage()
    {
        InitializeComponent();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_is3dLoaded)
        {
            return;
        }

        await LoadPokeBall3DAsync();
    }

    private async void OnSeeActionClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AnimationPage());
    }

    private async Task LoadPokeBall3DAsync()
    {
        if (_is3dLoaded)
        {
            return;
        }

        try
        {
            await using var modelStream = await FileSystem.OpenAppPackageFileAsync("pokemon_basic_pokeball.glb");
            using var memoryStream = new MemoryStream();
            await modelStream.CopyToAsync(memoryStream);

            var modelBase64 = Convert.ToBase64String(memoryStream.ToArray());
            var html = $$"""
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <script type="module" src="https://ajax.googleapis.com/ajax/libs/model-viewer/3.4.0/model-viewer.min.js"></script>
    <style>
        html, body {
            margin: 0;
            width: 100%;
            height: 100%;
            background: transparent;
            overflow: hidden;
        }
        model-viewer {
            width: 100%;
            height: 100%;
            --poster-color: transparent;
        }
    </style>
</head>
<body>
    <model-viewer
        src="data:model/gltf-binary;base64,{{modelBase64}}"
        alt="Pokeball 3D"
        auto-rotate
        camera-controls
        disable-pan
        disable-zoom
        interaction-prompt="auto"
        shadow-intensity="1"
        touch-action="none">
    </model-viewer>
</body>
</html>
""";

            PokeBallWebView.Source = new HtmlWebViewSource { Html = html };
            _is3dLoaded = true;
        }
        catch (Exception)
        {
            PokeBallWebView.Source = new HtmlWebViewSource
            {
                Html = "<html><body style='font-family:sans-serif;text-align:center;padding:24px;'>Impossible de charger la Pokeball 3D.</body></html>"
            };
            _is3dLoaded = false;
        }
    }
}
