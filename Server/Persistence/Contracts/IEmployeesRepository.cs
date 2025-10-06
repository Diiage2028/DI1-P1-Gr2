using Server.Models;

namespace Server.Persistence.Contracts;

public interface IEmployeesRepository
{
    Task SaveEmployee(Employee employee);
    Task<List<Employee>> GetEmployeesByGameId(int gameId);
}
