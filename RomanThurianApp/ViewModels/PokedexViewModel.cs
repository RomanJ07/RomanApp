using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanThurianApp.Models;
using RomanThurianApp.Services;

namespace RomanThurianApp.ViewModels;

public partial class PokedexViewModel : ObservableObject
{
    private readonly IPokeApiService _pokeApiService;
    private readonly ICapturedPokemonService _capturedPokemonService;
    private CancellationTokenSource? _detailCts;
    private PokemonListItem? _selectedPokemon;
    private PokemonDetail? _selectedPokemonDetail;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public PokedexViewModel(IPokeApiService pokeApiService, ICapturedPokemonService capturedPokemonService)
    {
        _pokeApiService = pokeApiService;
        _capturedPokemonService = capturedPokemonService;
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

            if (value.IsCaptured)
            {
                SelectedPokemonDetail = new PokemonDetail
                {
                    Name = value.Name,
                    Description = string.IsNullOrWhiteSpace(value.CapturedDescription)
                        ? "Pokemon capture localement."
                        : value.CapturedDescription,
                    ImageUrl = string.Empty,
                    CapturedImage = value.DisplayImage,
                    Types = new List<string> { "capture" }
                };
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
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            IReadOnlyList<PokemonListItem> apiPokemons = Array.Empty<PokemonListItem>();
            try
            {
                apiPokemons = await _pokeApiService.GetPokemonsAsync(50);
            }
            catch (Exception)
            {
                ErrorMessage = "Impossible de charger les Pokemons en ligne. Affichage des captures locales.";
            }

            var capturedPokemons = await _capturedPokemonService.LoadCapturedPokemonsAsync();
            var capturedItems = capturedPokemons
                .OrderByDescending(p => p.CaptureDate)
                .Select(PokemonListItem.FromCapturedPokemon)
                .ToList();

            Pokemons.Clear();
            var displayNumber = 1;

            foreach (var pokemon in apiPokemons)
            {
                Pokemons.Add(new PokemonListItem
                {
                    Name = pokemon.Name,
                    Url = pokemon.Url,
                    DisplayNumber = displayNumber++
                });
            }

            foreach (var pokemon in capturedItems)
            {
                Pokemons.Add(new PokemonListItem
                {
                    Name = pokemon.Name,
                    Url = pokemon.Url,
                    IsCaptured = true,
                    CapturedPhotoData = pokemon.CapturedPhotoData,
                    CapturedDescription = pokemon.CapturedDescription,
                    CaptureDate = pokemon.CaptureDate,
                    DisplayNumber = displayNumber++
                });
            }

            if (Pokemons.Count == 0)
            {
                ErrorMessage = "Aucun Pokemon disponible.";
            }
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
