using PokeApiNet;
using RomanApp.Models;

namespace RomanApp.Services;

public class PokeApiService : IPokeApiService
{
    private readonly PokeApiClient _pokeApiClient;

    public PokeApiService(PokeApiClient pokeApiClient)
    {
        _pokeApiClient = pokeApiClient;
    }

    public async Task<IReadOnlyList<PokemonListItem>> GetPokemonsAsync(int limit, CancellationToken cancellationToken = default)
    {
        var page = await _pokeApiClient.GetNamedResourcePageAsync<Pokemon>(limit: limit, offset: 0);

        return page.Results
            .Where(p => !string.IsNullOrWhiteSpace(p.Name) && p.Url is not null)
            .Select(p => new PokemonListItem
            {
                Name = p.Name,
                Url = p.Url.ToString()
            })
            .ToList();
    }

    public async Task<PokemonDetail?> GetPokemonDetailAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var pokemon = await _pokeApiClient.GetResourceAsync<Pokemon>(name.ToLowerInvariant());
        var species = await _pokeApiClient.GetResourceAsync<PokemonSpecies>(name.ToLowerInvariant());

        var description = species.FlavorTextEntries?
            .FirstOrDefault(x => string.Equals(x.Language.Name, "fr", StringComparison.OrdinalIgnoreCase))?.FlavorText
            ?? species.FlavorTextEntries?
                .FirstOrDefault(x => string.Equals(x.Language.Name, "en", StringComparison.OrdinalIgnoreCase))?.FlavorText
            ?? "Description non disponible.";

        return new PokemonDetail
        {
            Name = pokemon.Name,
            ImageUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{pokemon.Id}.png",
            Description = CleanFlavorText(description)
        };
    }

    private static string CleanFlavorText(string input)
    {
        return input
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\f", " ", StringComparison.Ordinal)
            .Trim();
    }
}
