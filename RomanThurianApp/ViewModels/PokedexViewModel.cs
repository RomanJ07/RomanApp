using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanThurianApp.Models;
using RomanThurianApp.Services;

namespace RomanThurianApp.ViewModels;

public partial class PokedexViewModel : ObservableObject
{
    private readonly IPokeApiService _pokeApiService;
    private CancellationTokenSource? _detailCts;
    private bool _hasLoaded;
    private PokemonListItem? _selectedPokemon;
    private PokemonDetail? _selectedPokemonDetail;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public PokedexViewModel(IPokeApiService pokeApiService)
    {
        _pokeApiService = pokeApiService;
    }

    public ObservableCollection<PokemonListItem> Pokemons { get; } = new();

    public PokemonListItem? SelectedPokemon
    {
        get => _selectedPokemon;
        set
        {
            if (!SetProperty(ref _selectedPokemon, value))
            {
                return;
            }

            if (value is null)
            {
                SelectedPokemonDetail = null;
                return;
            }

            _ = LoadPokemonDetailSafeAsync(value.Name);
        }
    }

    public PokemonDetail? SelectedPokemonDetail
    {
        get => _selectedPokemonDetail;
        set
        {
            if (SetProperty(ref _selectedPokemonDetail, value))
            {
                OnPropertyChanged(nameof(HasSelectedPokemon));
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasSelectedPokemon => SelectedPokemonDetail is not null;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

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
