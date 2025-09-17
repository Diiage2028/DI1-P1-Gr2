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
}