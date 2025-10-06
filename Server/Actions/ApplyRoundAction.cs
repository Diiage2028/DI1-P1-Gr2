using FluentResults;
using FluentValidation;
using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record ApplyRoundActionParams(
    RoundAction RoundAction,
    int? GameId = null,
    Game? Game = null
);

public class ApplyRoundActionValidator : AbstractValidator<ApplyRoundActionParams>
{
    public ApplyRoundActionValidator()
    {
        RuleFor(p => p.RoundAction).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class ApplyRoundAction(
    IGamesRepository gamesRepository,
    ICompaniesRepository companiesRepository,
    IAction<CreateEmployeeParams, Result<Employee>> createEmployeeAction
        ) : IAction<ApplyRoundActionParams, Result>
{
    public async Task<Result> PerformAsync(ApplyRoundActionParams actionParams)
    {
        var actionValidator = new ApplyRoundActionValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (roundAction, gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        if (roundAction.PlayerId == null)
        {
            return Result.Fail("PlayerId is required for round action.");
        }

        var company = await companiesRepository.GetByPlayerId((int)roundAction.PlayerId);

        // Apply the specific round action based on type
        Result result;
        switch (roundAction)
        {
            case EnrollEmployeeRoundAction enrollEmployeeAction:
                var createParams = new CreateEmployeeParams("John Smith", GameId : gameId!.Value);
                var createResult = await createEmployeeAction.PerformAsync(createParams);
                result = createResult.IsSuccess
                    ? Result.Ok()
                    : Result.Fail(createResult.Errors);
                break;
            default:
                result = Result.Fail($"Unknown round action type: {roundAction.Type}");
                break;
        }

        return result;
    }
}
