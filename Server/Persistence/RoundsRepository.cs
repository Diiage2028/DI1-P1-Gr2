using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class RoundsRepository(WssDbContext context) : IRoundsRepository
{
    public async Task<Round?> GetById(int roundId)
    {
        return await context.Rounds
            .Include(r => r.Game)
            .ThenInclude(g => g.Players)
            .Include(r => r.Game)
            .ThenInclude(g => g.RoundsCollection)
            .Include(r => r.Actions) // This now loads from a separate table
            .Where(r => r.Id == roundId)
            .FirstOrDefaultAsync();
    }

    public async Task SaveRound(Round round)
    {
        if (round.Id.HasValue)
        {
            var existingRound = await context.Rounds
                .Include(r => r.Actions) // Include the actions from the separate table
                .FirstOrDefaultAsync(r => r.Id == round.Id);

            if (existingRound != null)
            {
                // Update basic round properties
                context.Entry(existingRound).CurrentValues.SetValues(round);

                // Handle the Actions collection with proper relationship management
                await UpdateRoundActions(existingRound, round.Actions);
            }
            else
            {
                await context.AddAsync(round);
            }
        }
        else
        {
            await context.AddAsync(round);
        }

        await context.SaveChangesAsync();
    }

    private async Task UpdateRoundActions(Round existingRound, ICollection<RoundAction> newActions)
    {
        Console.WriteLine($"Processing {newActions.Count} actions:");

        // Remove actions that are no longer in the new collection
        var actionsToRemove = existingRound.Actions
            .Where(existingAction => newActions.All(newAction => newAction.Id != existingAction.Id)) // Fixed: HasValue
            .ToList();

        foreach (var actionToRemove in actionsToRemove)
        {
            existingRound.Actions.Remove(actionToRemove);
            context.Remove(actionToRemove);
        }

        // Update existing actions and add new ones
        foreach (var newAction in newActions.ToList())
        {
            Console.WriteLine($"Type: {newAction.Type}, PlayerId: {newAction.PlayerId}");
            Console.WriteLine($"Payload type: {newAction.GetType().Name}");

            if (newAction is SendEmployeeForTrainingRoundAction trainingAction)
            {
                Console.WriteLine($"EmployeeId: {trainingAction.Payload.EmployeeId}");
            }

            if (newAction.Id != null)
            {
                // Update existing action
                var existingAction = existingRound.Actions
                    .FirstOrDefault(a => a.Id == newAction.Id);

                if (existingAction != null)
                {
                    // Update the existing action
                    context.Entry(existingAction).CurrentValues.SetValues(newAction);
                }
            }
            else
            {
                // Add new action - set the foreign key
                if (existingRound.Id != null)
                {
                    newAction.RoundId = existingRound.Id.Value;
                }

                existingRound.Actions.Add(newAction);
            }
        }
    }
}
