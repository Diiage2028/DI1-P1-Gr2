
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateEmployeeParams(string EmployeeName, int? CompanyId = null, Company? Company = null);

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeParams>
{
    public CreateEmployeeValidator()
    {
        RuleFor(p => p.EmployeeName).NotEmpty();
        RuleFor(p => p.CompanyId).NotEmpty().When(p => p.Company is null);
        RuleFor(p => p.Company).NotEmpty().When(p => p.CompanyId is null);
    }
}

public class CreateEmployee(
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<CreateEmployeeParams, Result<Employee>>
{
    public async Task<Result<Employee>> PerformAsync(CreateEmployeeParams actionParams)
    {
        var rnd = new Random();

        var actionValidator = new CreateEmployeeValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (employeeName, companyId, company) = actionParams;

        company ??= await companiesRepository.GetById(companyId!.Value);

        if (company is null)
        {
            Result.Fail($"Company with Id \"{companyId}\" not found.");
        }

        // Create employee first to get the skills
        var employee = new Employee(employeeName, company!.Id!.Value, company!.Player.GameId, 0); // Start with 0 salary

        // Get random skills and assign random levels
        var randomSkills = await skillsRepository.GetRandomSkills(3);
        foreach (var randomSkill in randomSkills)
        {
            var skillLevel = rnd.Next(1, 11); // Skill level between 1-10
            employee.Skills.Add(new LeveledSkill(randomSkill.Name, skillLevel));
        }

        // Calculate salary based on skill levels: 150 * skill level * 1.02 for each skill
        double totalSalary = 0;
        // foreach (var skill in employee.Skills)
        // {
        //     totalSalary += 20.0 * skill.Level * 1.02;
        // }

        // New formula: 200 * number of skills * average skill level * (1 - random factor up to 10%)
        totalSalary = 200 * employee.Skills.Count * employee.Skills.Average(s => s.Level) * (1 - rnd.NextDouble() * 0.1);

        // Set the calculated salary
        employee.Salary = totalSalary;

        await employeesRepository.SaveEmployee(employee);

        await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);

        return Result.Ok(employee);
    }
}
