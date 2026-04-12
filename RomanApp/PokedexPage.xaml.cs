using RomanApp.Models;
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

    private void OnPokemonSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PokemonListItem selectedPokemon)
        {
            _viewModel.SelectedPokemon = selectedPokemon;
        }
    }

    private void OnReturnToListClicked(object? sender, EventArgs e)
    {
        _viewModel.SelectedPokemonDetail = null;
        PokemonCollectionView.SelectedItem = null;
    }
}
