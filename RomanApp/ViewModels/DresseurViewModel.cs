using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanApp.Models;
using RomanApp.Services;

namespace RomanApp.ViewModels;

public partial class DresseurViewModel : ObservableObject
{
    private const int MaxTeamSize = 6;
    private readonly IPokeApiService _pokeApiService;
    private readonly ITrainerTeamRepository _trainerTeamRepository;
    private CancellationTokenSource? _searchDebounceCts;
    private bool _isTeamLoaded;

    
    public DresseurViewModel(IPokeApiService pokeApiService, ITrainerTeamRepository trainerTeamRepository)
    {
        _pokeApiService = pokeApiService;
        _trainerTeamRepository = trainerTeamRepository;

        for (var slotIndex = 1; slotIndex <= MaxTeamSize; slotIndex++)
        {
            var slot = new TrainerTeamSlot(slotIndex);
            slot.PropertyChanged += OnSlotPropertyChanged;
            TeamSlots.Add(slot);
        }
    }

    public ObservableCollection<TrainerTeamSlot> TeamSlots { get; } = new();

    public ObservableCollection<PokemonListItem> SearchResults { get; } = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        _ = DebounceSearchAsync(value);
    }

    public int TotalHp => TeamSlots.Sum(slot => slot.Hp);

    public int TotalAttack => TeamSlots.Sum(slot => slot.Attack);

    public bool HasAtLeastOneMember => TeamSlots.Any(slot => !slot.IsEmpty);

    [RelayCommand]
    private async Task LoadSavedTeamAsync()
    {
        if (_isTeamLoaded)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var members = await _trainerTeamRepository.LoadTeamAsync();
            foreach (var slot in TeamSlots)
            {
                var member = members.FirstOrDefault(item => item.SlotNumber == slot.SlotNumber);
                slot.Pokemon = member is null
                    ? null
                    : new PokemonDetail
                    {
                        Name = member.Name,
                        Description = member.Description,
                        ImageUrl = member.ImageUrl,
                        Hp = member.Hp,
                        Attack = member.Attack
                    };
            }

            StatusMessage = members.Count > 0
                ? "Equipe restauree depuis la base locale."
                : "Aucune equipe sauvegardee pour le moment.";
            _isTeamLoaded = true;
            RefreshComputedState();
        }
        catch (Exception)
        {
            StatusMessage = "Impossible de charger l'equipe sauvegardee.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchPokemonAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            StatusMessage = "Saisis un nom de Pokemon pour lancer la recherche.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var allPokemons = await _pokeApiService.GetPokemonsAsync(151);
            var filtered = allPokemons
                .Where(pokemon => pokemon.Name.Contains(SearchQuery.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(24)
                .ToList();

            SearchResults.Clear();
            foreach (var pokemon in filtered)
            {
                SearchResults.Add(pokemon);
            }

            StatusMessage = filtered.Count == 0
                ? "Aucun Pokemon trouve."
                : $"{filtered.Count} Pokemon(s) trouve(s).";
        }
        catch (Exception)
        {
            StatusMessage = "Erreur pendant la recherche Pokemon.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearTeam()
    {
        foreach (var slot in TeamSlots)
        {
            slot.Pokemon = null;
        }

        StatusMessage = "Equipe videe.";
        RefreshComputedState();
    }
    
    [RelayCommand]
    private async Task AddPokemonAsync(PokemonListItem? pokemon)
    {
        if (pokemon is null)
        {
            StatusMessage = "Selection invalide.";
            return;
        }

        var alreadyInTeam = TeamSlots.Any(slot => slot.Pokemon is not null
            && string.Equals(slot.Pokemon.Name, pokemon.Name, StringComparison.OrdinalIgnoreCase));
        if (alreadyInTeam)
        {
            StatusMessage = "Ce Pokemon est deja dans ton equipe.";
            return;
        }

        var emptySlot = TeamSlots.FirstOrDefault(slot => slot.IsEmpty);
        if (emptySlot is null)
        {
            StatusMessage = "Ton equipe est deja complete (6 Pokemon).";
            return;
        }

        try
        {
            IsBusy = true;
            var detail = await _pokeApiService.GetPokemonDetailAsync(pokemon.Name);

            if (detail is null)
            {
                StatusMessage = "Impossible de recuperer les details du Pokemon.";
                return;
            }

            emptySlot.Pokemon = detail;
            StatusMessage = $"{detail.DisplayName} ajoute a l'emplacement {emptySlot.SlotNumber}.";
            RefreshComputedState();
        }
        catch (Exception)
        {
            StatusMessage = "Erreur pendant l'ajout du Pokemon.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RemovePokemon(TrainerTeamSlot? slot)
    {
        if (slot?.Pokemon is null)
        {
            return;
        }

        var removedName = slot.Pokemon.DisplayName;
        slot.Pokemon = null;
        StatusMessage = $"{removedName} retire de l'equipe.";
        RefreshComputedState();
    }
    
    [RelayCommand]
    private async Task SaveTeamAsync()
    {
        try
        {
            IsBusy = true;

            var members = TeamSlots
                .Where(slot => slot.Pokemon is not null)
                .Select(slot => new TrainerTeamMember
                {
                    SlotNumber = slot.SlotNumber,
                    Name = slot.Pokemon!.Name,
                    Description = slot.Pokemon.Description,
                    ImageUrl = slot.Pokemon.ImageUrl,
                    Hp = slot.Pokemon.Hp,
                    Attack = slot.Pokemon.Attack
                })
                .ToList();

            await _trainerTeamRepository.SaveTeamAsync(members);
            StatusMessage = "Equipe sauvegardee en local.";
        }
        catch (Exception)
        {
            StatusMessage = "Impossible de sauvegarder l'equipe en local.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnSlotPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TrainerTeamSlot.Pokemon))
        {
            RefreshComputedState();
        }
    }

    private void RefreshComputedState()
    {
        OnPropertyChanged(nameof(TotalHp));
        OnPropertyChanged(nameof(TotalAttack));
        OnPropertyChanged(nameof(HasAtLeastOneMember));
    }

    private async Task DebounceSearchAsync(string query)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();

        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            return;
        }

        var cts = new CancellationTokenSource();
        _searchDebounceCts = cts;

        try
        {
            await Task.Delay(350, cts.Token);
            await SearchPokemonAsync();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_searchDebounceCts, cts))
            {
                _searchDebounceCts = null;
            }

            cts.Dispose();
        }
    }
}


