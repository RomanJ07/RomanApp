using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanApp.Models;
using RomanApp.Services;

namespace RomanApp.ViewModels;

public partial class TrainerViewModel : ObservableObject
{
    private const int MaxTeamSize = 6;
    private readonly IPokeApiService _pokeApiService;
    private readonly ITrainerTeamRepository _trainerTeamRepository;
    private CancellationTokenSource? _searchDebounceCts;
    private bool _isTeamLoaded;
    private string _searchQuery = string.Empty;
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    public TrainerViewModel(IPokeApiService pokeApiService, ITrainerTeamRepository trainerTeamRepository)
    {
        _pokeApiService = pokeApiService;
        _trainerTeamRepository = trainerTeamRepository;

        foreach (var slotIndex in Enumerable.Range(1, MaxTeamSize))
        {
            var slot = new TrainerTeamSlot(slotIndex);
            slot.PropertyChanged += OnSlotPropertyChanged;
            TeamSlots.Add(slot);
        }
    }

    public ObservableCollection<TrainerTeamSlot> TeamSlots { get; } = new();

    public ObservableCollection<PokemonListItem> SearchResults { get; } = new();

    public ObservableCollection<TeamTypeCountItem> TypeDistribution { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                _ = DebounceSearchAsync(value);
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int TotalHp => TeamSlots.Sum(slot => slot.Hp);

    public int TotalAttack => TeamSlots.Sum(slot => slot.Attack);

    public int CurrentTeamCount => TeamSlots.Count(slot => !slot.IsEmpty);

    public int EmptySlotsCount => MaxTeamSize - CurrentTeamCount;

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

            var membersBySlot = (await _trainerTeamRepository.LoadTeamAsync())
                .ToDictionary(member => member.SlotNumber);

            foreach (var slot in TeamSlots)
            {
                slot.Pokemon = membersBySlot.TryGetValue(slot.SlotNumber, out var member)
                    ? ToPokemonDetail(member)
                    : null;
            }

            StatusMessage = membersBySlot.Count > 0
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

            var filtered = (await _pokeApiService.GetPokemonsAsync(151))
                .Where(pokemon => pokemon.Name.Contains(SearchQuery.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(24)
                .ToList();

            ReplaceCollection(SearchResults, filtered);

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
    }

    [RelayCommand]
    private async Task SaveTeamAsync()
    {
        try
        {
            IsBusy = true;

            var members = TeamSlots
                .Where(slot => slot.Pokemon is not null)
                .Select(slot => ToTeamMember(slot.SlotNumber, slot.Pokemon!))
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
        OnPropertyChanged(nameof(CurrentTeamCount));
        OnPropertyChanged(nameof(EmptySlotsCount));
        OnPropertyChanged(nameof(HasAtLeastOneMember));

        var distribution = TeamSlots
            .Where(slot => slot.Pokemon is not null)
            .SelectMany(slot => slot.Pokemon!.Types)
            .Where(typeName => !string.IsNullOrWhiteSpace(typeName))
            .GroupBy(NormalizeTypeName)
            .Select(group => new TeamTypeCountItem
            {
                TypeName = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.TypeName)
            .ToList();

        ReplaceCollection(TypeDistribution, distribution);
    }

    private async Task DebounceSearchAsync(string query)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();

        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            StatusMessage = string.Empty;
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
            // Ignore: un nouveau texte annule la recherche precedente.
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

    private static PokemonDetail ToPokemonDetail(TrainerTeamMember member)
    {
        return new PokemonDetail
        {
            Name = member.Name,
            Description = member.Description,
            ImageUrl = member.ImageUrl,
            Hp = member.Hp,
            Attack = member.Attack,
            Types = member.Types
        };
    }

    private static TrainerTeamMember ToTeamMember(int slotNumber, PokemonDetail detail)
    {
        return new TrainerTeamMember
        {
            SlotNumber = slotNumber,
            Name = detail.Name,
            Description = detail.Description,
            ImageUrl = detail.ImageUrl,
            Hp = detail.Hp,
            Attack = detail.Attack,
            Types = detail.Types
        };
    }

    private static string NormalizeTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        var trimmed = typeName.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1).ToLowerInvariant();
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}

