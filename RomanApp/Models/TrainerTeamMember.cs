namespace RomanApp.Models;

public class TrainerTeamMember
{
    public int SlotNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int Hp { get; set; }

    public int Attack { get; set; }

    public List<string> Types { get; set; } = new();
}

