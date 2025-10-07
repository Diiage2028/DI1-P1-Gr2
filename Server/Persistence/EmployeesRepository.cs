using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class EmployeesRepository(WssDbContext context) : IEmployeesRepository
{
    public async Task SaveEmployee(Employee employee)
    {
        if (employee.Id is null)
        {
            await context.AddAsync(employee);
        }

        await context.SaveChangesAsync();
    }
    public async Task<List<Employee>> GetEmployeesByGameId(int gameId)
    {
        return await context.Employees
            .Where(e => e.GameId == gameId)
            .Include(e => e.Skills)
            .ToListAsync();
    }

    public async Task DeleteEmployee(Employee employee)
    {
        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }
}
