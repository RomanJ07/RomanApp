namespace RomanApp.Models
{
    public class PokemonDetail
    {
        public string Name { get; set; } = string.Empty;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return string.Empty;
                }

                if (Name.Length == 1)
                {
                    return Name.ToUpperInvariant();
                }

                return char.ToUpperInvariant(Name[0]) + Name.Substring(1);
            }
        }

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
}
