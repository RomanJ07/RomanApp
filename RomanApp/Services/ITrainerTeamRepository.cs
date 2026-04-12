using RomanApp.Models;

namespace RomanApp.Services;

public interface ITrainerTeamRepository
{
    Task SaveTeamAsync(IReadOnlyList<TrainerTeamMember> members);

    Task<IReadOnlyList<TrainerTeamMember>> LoadTeamAsync();
}

