﻿﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanApp.Models;
using RomanApp.Services;

namespace RomanApp.ViewModels;

public partial class PokedexViewModel : ObservableObject
{
    private readonly IPokeApiService _pokeApiService;
    private readonly ICapturedPokemonService _capturedPokemonService;
    private CancellationTokenSource? _detailCts;
    private bool _hasLoaded;

    public PokedexViewModel(IPokeApiService pokeApiService, ICapturedPokemonService capturedPokemonService)
    {
        _pokeApiService = pokeApiService;
        _capturedPokemonService = capturedPokemonService;
    }

    public ObservableCollection<PokemonListItem> Pokemons { get; } = new();

    [ObservableProperty]
    private ObservableCollection<CapturedPokemon> capturedPokemons = new();

    public ObservableCollection<Models.PokemonItem> AllPokemons { get; } = new();

    [ObservableProperty]
    private PokemonDetail? selectedPokemonDetail;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasSelectedPokemon => SelectedPokemonDetail is not null;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

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

            // Charger les Pokémons de l'API
            var pokemons = await _pokeApiService.GetPokemonsAsync(50);
            Pokemons.Clear();
            foreach (var pokemon in pokemons)
            {
                Pokemons.Add(pokemon);
            }

            // Charger les Pokémons capturés
            var capturedPokemons = await _capturedPokemonService.LoadCapturedPokemonsAsync();
            CapturedPokemons.Clear();
            foreach (var captured in capturedPokemons)
            {
                CapturedPokemons.Add(captured);
            }

            // Fusionner dans une seule liste AllPokemons
            AllPokemons.Clear();
            
            // Ajouter les Pokémons de l'API
            foreach (var pokemon in pokemons)
            {
                AllPokemons.Add(new Models.PokemonItem
                {
                    Id = pokemon.Name,
                    Title = pokemon.DisplayName,
                    ApiPokemon = pokemon,
                    IsCaptured = false
                });
            }

            // Ajouter les Pokémons capturés
            foreach (var captured in capturedPokemons)
            {
                AllPokemons.Add(new Models.PokemonItem
                {
                    Id = captured.Id,
                    Title = captured.Title,
                    Description = captured.Description,
                    PhotoData = captured.PhotoData,
                    CapturedPokemon = captured,
                    IsCaptured = true
                });
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

    [RelayCommand]
    public async Task SelectPokemonAsync(PokemonItem pokemon)
    {
        if (pokemon.IsCaptured)
        {
            // Pour les pokémons capturés, afficher les détails du pokémon capturé
            if (pokemon.CapturedPokemon != null)
            {
                SelectedPokemonDetail = new PokemonDetail
                {
                    Name = pokemon.CapturedPokemon.Title,
                    Description = pokemon.CapturedPokemon.Description,
                    CapturedImage = pokemon.DisplayImage
                };
            }
        }
        else
        {
            // Pour les pokémons de l'API, charger les détails
            await LoadPokemonDetailSafeAsync(pokemon.Id);
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
