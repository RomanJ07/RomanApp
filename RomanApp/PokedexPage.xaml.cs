using RomanApp.ViewModels;

namespace RomanApp;

public partial class PokedexPage
{
    private readonly PokedexViewModel _viewModel;
    private bool _hasLoaded;

    public PokedexPage(PokedexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        await _viewModel.LoadPokemonsCommand.ExecuteAsync(null);
    }
}
