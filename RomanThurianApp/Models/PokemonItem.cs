namespace RomanThurianApp.Models;

public class PokemonItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[]? PhotoData { get; set; }
    public bool IsCaptured { get; set; }
    
    public string ShortDescription
    {
        get
        {
            // Pour les Pokémons capturés, ne pas afficher la description dans la liste
            if (IsCaptured)
                return string.Empty;
            
            if (string.IsNullOrEmpty(Description) || Description.Length <= 100)
                return Description;
            return Description.Substring(0, 97) + "...";
        }
    }
    
    // Pour les Pokémons API
    public PokemonListItem? ApiPokemon { get; set; }
    
    // Pour les Pokémons capturés
    public CapturedPokemon? CapturedPokemon { get; set; }

    public ImageSource GetImageSource()
    {
        if (IsCaptured && PhotoData != null && PhotoData.Length > 0)
        {
            return ImageSource.FromStream(() => new MemoryStream(PhotoData));
        }
        else if (ApiPokemon != null)
        {
            return ApiPokemon.ImageUrl;
        }
        return null!;
    }

    public string ImageUrl
    {
        get
        {
            if (IsCaptured && PhotoData != null && PhotoData.Length > 0)
            {
                // Pour les pokémons capturés, on retourne null et on utilise GetImageSource() à la place
                return string.Empty;
            }
            return ApiPokemon?.ImageUrl ?? string.Empty;
        }
    }

    public ImageSource DisplayImage
    {
        get
        {
            if (IsCaptured && PhotoData != null && PhotoData.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(PhotoData));
            }
            else if (ApiPokemon != null && !string.IsNullOrEmpty(ApiPokemon.ImageUrl))
            {
                return ApiPokemon.ImageUrl;
            }
            return null!;
        }
    }
}


