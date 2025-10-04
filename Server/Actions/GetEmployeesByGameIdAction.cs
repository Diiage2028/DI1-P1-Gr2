using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using Server.Actions.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record GetEmployeesByGameIdParams(int GameId);
public class GetEmployeesByGameIdValidator : AbstractValidator<GetEmployeesByGameIdParams>
{
    public GetEmployeesByGameIdValidator()
    {
        RuleFor(x => x.GameId)
            .GreaterThan(0)
            .WithMessage("GameId must be a positive integer");
    }
}
public class GetEmployeesByGameIdAction : IAction<GetEmployeesByGameIdParams, Result<List<Employee>>>
{
    private readonly IEmployeesRepository _employeesRepository;
    public GetEmployeesByGameIdAction(IEmployeesRepository employeesRepository)
    {
        _employeesRepository = employeesRepository;
    }

    public async Task<Result<List<Employee>>> PerformAsync(GetEmployeesByGameIdParams actionParams)
    {
        // Valider les paramètres
        var validator = new GetEmployeesByGameIdValidator();
        var validationResult = await validator.ValidateAsync(actionParams);

        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            var employees = await _employeesRepository.GetEmployeesByGameId(actionParams.GameId);
            return Result.Ok(employees);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve employees: {ex.Message}");
        }
        
    }
}
