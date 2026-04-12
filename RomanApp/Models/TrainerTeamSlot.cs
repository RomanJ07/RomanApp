using CommunityToolkit.Mvvm.ComponentModel;

namespace RomanApp.Models;

public partial class TrainerTeamSlot : ObservableObject
{
    private PokemonDetail? _pokemon;

    public TrainerTeamSlot(int slotNumber)
    {
        SlotNumber = slotNumber;
    }

    public int SlotNumber { get; }

    public PokemonDetail? Pokemon
    {
        get => _pokemon;
        set
        {
            if (SetProperty(ref _pokemon, value))
            {
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(PokemonDisplayName));
                OnPropertyChanged(nameof(PokemonImageUrl));
                OnPropertyChanged(nameof(Hp));
                OnPropertyChanged(nameof(Attack));
            }
        }
    }

    public bool IsEmpty => Pokemon is null;

    public string PokemonDisplayName => Pokemon?.DisplayName ?? "Slot vide";

    public string PokemonImageUrl => Pokemon?.ImageUrl ?? string.Empty;

    public int Hp => Pokemon?.Hp ?? 0;

    public int Attack => Pokemon?.Attack ?? 0;

}

