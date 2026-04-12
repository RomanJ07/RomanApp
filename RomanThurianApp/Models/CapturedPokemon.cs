namespace RomanThurianApp.Models;

public class CapturedPokemon
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string PhotoPath { get; set; } = string.Empty;
    
    public DateTime CaptureDate { get; set; } = DateTime.Now;
    
    public byte[]? PhotoData { get; set; }
}

