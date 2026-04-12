using RomanThurianApp.Models;

namespace RomanThurianApp.Services;

public interface IPokeApiService
{
    Task<IReadOnlyList<PokemonListItem>> GetPokemonsAsync(int limit, CancellationToken cancellationToken = default);

    Task<PokemonDetail?> GetPokemonDetailAsync(string name, CancellationToken cancellationToken = default);
}

