using Server.Models;

namespace Server.Persistence.Contracts;

public interface ITrainingRepository
{
    Task<List<Training>> GetTrainings();
}
