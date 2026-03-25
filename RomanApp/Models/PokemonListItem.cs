namespace RomanApp.Models;

public class PokemonListItem
{
    public string Name { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string ImageUrl => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{Id}.png";

    public string DisplayName => string.IsNullOrWhiteSpace(Name)
        ? string.Empty
        : Name.Length == 1
            ? Name.ToUpperInvariant()
            : char.ToUpperInvariant(Name[0]) + Name.Substring(1);

    public int Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return 0;
            }

            var trimmed = Url.TrimEnd('/');
            var separatorIndex = trimmed.LastIndexOf('/');
            var lastSegment = separatorIndex >= 0 ? trimmed.Substring(separatorIndex + 1) : trimmed;
            return int.TryParse(lastSegment, out var id) ? id : 0;
        }
    }
}
