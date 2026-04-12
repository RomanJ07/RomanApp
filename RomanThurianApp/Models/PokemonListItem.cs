namespace RomanThurianApp.Models;

public class PokemonListItem
{
    public string Name { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public bool IsCaptured { get; init; }

    public byte[]? CapturedPhotoData { get; init; }

    public string CapturedDescription { get; init; } = string.Empty;

    public DateTime? CaptureDate { get; init; }

    public int DisplayNumber { get; init; }

    public string ImageUrl => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{Id}.png";

    public string DisplayName => string.IsNullOrWhiteSpace(Name)
        ? string.Empty
        : Name.Length == 1
            ? Name.ToUpperInvariant()
            : char.ToUpperInvariant(Name[0]) + Name.Substring(1);

    public string Subtitle
    {
        get
        {
            if (DisplayNumber > 0)
            {
                return $"Pokemon #{DisplayNumber}";
            }

            return IsCaptured ? "Pokemon" : $"Pokemon #{Id}";
        }
    }

    public ImageSource? DisplayImage
    {
        get
        {
            if (IsCaptured && CapturedPhotoData is { Length: > 0 })
            {
                return ImageSource.FromStream(() => new MemoryStream(CapturedPhotoData));
            }

            if (Id <= 0)
            {
                return null;
            }

            return ImageSource.FromUri(new Uri(ImageUrl));
        }
    }

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

    public static PokemonListItem FromCapturedPokemon(CapturedPokemon captured)
    {
        return new PokemonListItem
        {
            Name = captured.Title,
            Url = string.Empty,
            IsCaptured = true,
            CapturedPhotoData = captured.PhotoData,
            CapturedDescription = captured.Description,
            CaptureDate = captured.CaptureDate
        };
    }
}
