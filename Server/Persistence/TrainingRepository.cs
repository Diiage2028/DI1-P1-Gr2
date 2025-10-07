using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class TrainingRepository(WssDbContext context) : ITrainingRepository
{
    public async Task<List<Training>> GetTrainings()
    {
        return await context.Training
            .Include(t => t.Skill)
            .ToListAsync();
    }
}
