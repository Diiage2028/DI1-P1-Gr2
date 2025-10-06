using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class CompaniesRepository(WssDbContext context) : ICompaniesRepository
{
    public async Task<bool> CompanyExists(int companyId)
    {
        return await context.Companies.AnyAsync(company => company.Id == companyId);
    }

    public async Task<bool> IsCompanyNameAvailable(string companyName, int gameId)
    {
        return !await context.Companies
          .Join(
            context.Players,
            company => company.PlayerId,
            player => player.Id,
            (company, player) => new { Company = company, Player = player }
          )
          .Where(res => res.Player.GameId == gameId)
          .AnyAsync(res => res.Company.Name == companyName);
    }

    public async Task<Company?> GetById(int companyId)
    {
        return await context.Companies.FindAsync(companyId);
    }

    public async Task SaveCompany(Company company)
    {
        if (company.Id is null)
        {
            await context.AddAsync(company);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Company?> GetByPlayerId(int playerId)
    {
        return await context.Companies.FirstOrDefaultAsync(c => c.PlayerId == playerId);
    }
}
