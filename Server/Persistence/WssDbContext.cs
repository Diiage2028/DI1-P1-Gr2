using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Server.Models;

namespace Server.Persistence;

public class WssDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Consultant> Consultants { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // Creates instance of connexion
    {
        var dbOptions = configuration.GetSection("Database");

        var dbHost = dbOptions.GetValue<string>("Host");
        var dbPort = dbOptions.GetValue<string>("Port");
        var dbName = dbOptions.GetValue<string>("Name");
        var dbUser = dbOptions.GetValue<string>("User");
        var dbPass = dbOptions.GetValue<string>("Pass");

        var dbConnectionString = $"Host={dbHost};Port={dbPort};Db={dbName};Username={dbUser};Password={dbPass}";

        optionsBuilder.UseNpgsql(dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) // writes relations between classes
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Treasury).HasColumnType("integer").HasDefaultValue(1000000); // Default value
            e.HasOne(e => e.Player)
                .WithOne(e => e.Company)
                .HasForeignKey<Company>(e => e.PlayerId) // relation 1 1
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // if delete player, company gets deleted
            e.HasMany(e => e.Employees)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<Consultant>(e =>
        {
            e.ToTable("consultants");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("employees");
            e.HasKey(e => e.Id);
            e.Property(e => e.Salary).HasColumnType("integer");
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Rounds).HasColumnType("integer");
            e.Property(e => e.Status)
                .HasColumnType("varchar(255)")
                .HasDefaultValue(GameStatus.Waiting)
                .HasConversion(new EnumToStringConverter<GameStatus>());
            e.HasMany(e => e.Players)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
            e.HasMany(e => e.Consultants)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
            e.HasMany(e => e.RoundsCollection)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.GameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithOne(e => e.Player)
                .HasForeignKey<Company>(e => e.PlayerId);
        });

        modelBuilder.Entity<Round>(e =>
        {
            e.ToTable("rounds");
            e.HasKey(e => e.Id);
            e.Property(e => e.Order).HasColumnType("integer");
            e.HasOne(e => e.Game)
                .WithMany(e => e.RoundsCollection)
                .HasForeignKey(e => e.GameId);

            // Regular one-to-many instead of JSON
            e.HasMany(e => e.Actions)
                .WithOne()
                .HasForeignKey(a => a.RoundId) // You'll need to add this property
                .IsRequired();
        });

        modelBuilder.Entity<RoundAction>(e =>
        {
            e.ToTable("round_actions");
            e.HasKey(e => e.Id);

            // Configure TPH (Table Per Hierarchy) for inheritance
            e.HasDiscriminator<RoundActionType>("ActionType")
                .HasValue<RoundAction>(RoundActionType.Default) // Add default for base class
                .HasValue<SendEmployeeForTrainingRoundAction>(RoundActionType.SendEmployeeForTraining)
                .HasValue<ParticipateInProjectRoundAction>(RoundActionType.ParticipateInProject)
                .HasValue<EnrollEmployeeRoundAction>(RoundActionType.EnrollEmployee)
                .HasValue<FireAnEmployeeRoundAction>(RoundActionType.FireAnEmployee)
                .HasValue<ConfirmRoundAction>(RoundActionType.ConfirmRound);

            // Configure the foreign key property
            e.Property(a => a.RoundId).IsRequired();
        });

        // Configure derived types separately
        modelBuilder.Entity<SendEmployeeForTrainingRoundAction>()
            .Property(a => a.Payload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<ParticipateInProjectRoundAction>()
            .Property(a => a.Payload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<FireAnEmployeeRoundAction>()
            .Property(a => a.Payload)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasData(
                new Skill("HTML") { Id = 1 },
                new Skill("CSS") { Id = 2 },
                new Skill("JavaScript") { Id = 3 },
                new Skill("TypeScript") { Id = 4 },
                new Skill("React") { Id = 5 },
                new Skill("Angular") { Id = 6 },
                new Skill("Vue.js") { Id = 7 },
                new Skill("Node.js") { Id = 8 },
                new Skill("Express.js") { Id = 9 },
                new Skill("ASP.NET Core") { Id = 10 },
                new Skill("Ruby on Rails") { Id = 11 },
                new Skill("Django") { Id = 12 },
                new Skill("Flask") { Id = 13 },
                new Skill("PHP") { Id = 14 },
                new Skill("Laravel") { Id = 15 },
                new Skill("Spring Boot") { Id = 16 },
                new Skill("SQL") { Id = 17 },
                new Skill("NoSQL") { Id = 18 },
                new Skill("GraphQL") { Id = 19 },
                new Skill("REST APIs") { Id = 20 }
            );
        });
    }
}
