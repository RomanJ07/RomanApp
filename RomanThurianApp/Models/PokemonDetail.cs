﻿namespace RomanThurianApp.Models
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

        public int Hp { get; set; }

        public int Attack { get; set; }

        public List<string> Types { get; set; } = new();

        // Pour les images capturées (données binaires)
        public ImageSource? CapturedImage { get; set; }

        // Propriété de convenance pour obtenir l'image (capturée ou URL)
        public ImageSource DisplayImage
        {
            get
            {
                if (CapturedImage != null)
                {
                    return CapturedImage;
                }
                if (!string.IsNullOrEmpty(ImageUrl))
                {
                    return ImageUrl;
                }
                return null!;
            }
        }
    }
}

