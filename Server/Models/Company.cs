using Server.Hubs.Records;

namespace Server.Models;

public class Company(string name, int playerId) //Constructor, when we create a client we need to assign it a name and id
{
    public int? Id { get; private set; } // given by bdd, can't set it, can be null
    // when a class has an Id as an attribute it is automatically a primary key

    public string Name { get; set; } = name;

    public int PlayerId { get; set; } = playerId; // is taken automatically from the Player table thanks to the framework Entity Framework Core (EFCore)
    // foreign key automatically set for Player thanks to naming conventions

    public Player Player { get; set; } = null!; // relation to table Player

    public int Treasury { get; set; } = 1000000;

    public ICollection<Employee> Employees { get; } = []; // convention: creates instance of a list

    public CompanyOverview ToOverview()
    {
        return new CompanyOverview(
            Id is null ? 0 : (int) Id, Name,
            Treasury, Employees.Select(e => e.ToOverview()).ToList()
        );
    }
}
