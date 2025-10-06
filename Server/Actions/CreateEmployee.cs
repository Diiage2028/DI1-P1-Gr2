
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;
// Immutable record (DTO) holding parameters needed to create an Employee.
// Either a CompanyId or a Company object must be provided.
public sealed record CreateEmployeeParams(string EmployeeName, int GameId );

// Validator that defines rules for CreateEmployeeParams using FluentValidation.
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeParams>
{
    public CreateEmployeeValidator()
    {
        // Employee name must not be empty
        RuleFor(p => p.EmployeeName).NotEmpty();

        // If no Company object is provided, CompanyId must be provided
        //RuleFor(p => p.CompanyId).NotEmpty().When(p => p.Company is null);

        // If no CompanyId is provided, Company object must be provided
        //RuleFor(p => p.Company).NotEmpty().When(p => p.CompanyId is null);
    }
}

// Action class responsible for the business logic of creating an Employee.
// Implements IAction interface: input = CreateEmployeeParams, output = Result<Employee>.
public class CreateEmployee(
    //ICompaniesRepository companiesRepository,   // Access company data
    IEmployeesRepository employeesRepository,   // Save employee data
    ISkillsRepository skillsRepository,         // Retrieve skills
    IGameHubService gameHubService              // Notify clients via SignalR
) : IAction<CreateEmployeeParams, Result<Employee>>
{
    // Core business logic for creating an employee
    public async Task<Result<Employee>> PerformAsync(CreateEmployeeParams actionParams)
    {
        var rnd = new Random();

        // Validate input parameters with the validator
        var actionValidator = new CreateEmployeeValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        // If validation fails, return failure result with error messages
        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        // Deconstruct parameters for easier access
        var (employeeName, gameId) = actionParams;

        
        

        // Create employee first to get the skills
        var employee = new Employee(employeeName, null, gameId, 0); // Start with 0 salary

        // Get random skills and assign random levels
        var randomSkills = await skillsRepository.GetRandomSkills(3);
        foreach (var randomSkill in randomSkills)
        {
            var skillLevel = rnd.Next(1, 11); // Skill level between 1-10
            employee.Skills.Add(new LeveledSkill(randomSkill.Name, skillLevel));
        }

        decimal totalSalary;

        if (employee.Skills.Count == 0)
        {
            return Result.Fail("No skills available to compute salary.");
        }
        // New formula: baseFactor * count * avgLevel * (1 - random up to 10%)
        const double baseFactor = 200;
        var avgLevel = employee.Skills.Average(s => s.Level);
        totalSalary = new decimal(baseFactor * employee.Skills.Count * avgLevel * (1 - rnd.NextDouble() * 0.1));
        // Set the calculated salary
        employee.Salary = totalSalary;

        // Save the new employee in the repository
        await employeesRepository.SaveEmployee(employee);

        // Notify clients that the game state has changed
        await gameHubService.UpdateCurrentGame(gameId: gameId);

        // Return success with the created employee
        return Result.Ok(employee);
    }
}
