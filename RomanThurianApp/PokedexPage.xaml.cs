using RomanThurianApp.Models;
using RomanThurianApp.ViewModels;

namespace RomanThurianApp;

public partial class PokedexPage
{
    private readonly PokedexViewModel _viewModel;
     
    
    public PokedexPage(PokedexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
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
