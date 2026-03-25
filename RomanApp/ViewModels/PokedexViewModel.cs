using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanApp.Models;
using RomanApp.Services;

namespace RomanApp.ViewModels;

public partial class PokedexViewModel : ObservableObject
{
    private readonly IPokeApiService _pokeApiService;
    private CancellationTokenSource? _detailCts;
    private bool _hasLoaded;

    public PokedexViewModel(IPokeApiService pokeApiService)
    {
        _pokeApiService = pokeApiService;
    }

    public ObservableCollection<PokemonListItem> Pokemons { get; } = new();

    [ObservableProperty]
    private PokemonListItem? selectedPokemon;

    [ObservableProperty]
    private PokemonDetail? selectedPokemonDetail;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasSelectedPokemon => SelectedPokemonDetail is not null;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    partial void OnSelectedPokemonChanged(PokemonListItem? value)
    {
        if (value is null)
        {
            SelectedPokemonDetail = null;
            return;
        }

        _ = LoadPokemonDetailSafeAsync(value.Name);
    }

    partial void OnSelectedPokemonDetailChanged(PokemonDetail? value)
    {
        OnPropertyChanged(nameof(HasSelectedPokemon));
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    [RelayCommand]
    public async Task LoadPokemonsAsync()
    {
        if (_hasLoaded)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var pokemons = await _pokeApiService.GetPokemonsAsync(50);
            Pokemons.Clear();
            foreach (var pokemon in pokemons)
            {
                Pokemons.Add(pokemon);
            }

            _hasLoaded = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger le Pokedex. Verifie ta connexion internet.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPokemonDetailSafeAsync(string pokemonName)
    {
        try
        {
            await LoadPokemonDetailAsync(pokemonName);
        }
        catch (Exception)
        {
            SelectedPokemonDetail = null;
            ErrorMessage = "Impossible de charger le detail du Pokemon selectionne.";
        }
    }

    private async Task LoadPokemonDetailAsync(string pokemonName)
    {
        _detailCts?.Cancel();
        _detailCts?.Dispose();
        _detailCts = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var detail = await _pokeApiService.GetPokemonDetailAsync(pokemonName, _detailCts.Token);
            SelectedPokemonDetail = detail;
        }
        catch (OperationCanceledException)
        {
            // Ignore si une nouvelle selection est faite rapidement.
        }
        finally
        {
            IsLoading = false;
        }
    }
}
