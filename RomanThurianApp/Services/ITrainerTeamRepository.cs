using RomanThurianApp.Models;

namespace RomanThurianApp.Services;

public interface ITrainerTeamRepository
{
    Task SaveTeamAsync(IReadOnlyList<TrainerTeamMember> members);

    Task<IReadOnlyList<TrainerTeamMember>> LoadTeamAsync();
}

