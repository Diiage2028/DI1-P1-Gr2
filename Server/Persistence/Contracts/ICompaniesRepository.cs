using Server.Models;

namespace Server.Persistence.Contracts;

public interface ICompaniesRepository
{
    Task<bool> IsCompanyNameAvailable(string companyName, int gameId);
    Task<bool> CompanyExists(int companyId);
    Task<Company?> GetById(int companyId);
    Task SaveCompany(Company company);
    Task<Company?> GetByPlayerId(int playerId);
}
