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
    public DbSet<Training> Training { get; set; } = null!;
    public DbSet<InTraining> InTraining { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // Creates instance of connexion
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPass = Environment.GetEnvironmentVariable("DB_PASS");

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
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Salary).HasColumnType("integer");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired(false)
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
            e.OwnsMany(e => e.Actions, builder => builder.ToJson());
        });

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
        modelBuilder.Entity<Training>(e =>
        {
            e.ToTable("trainings");
            e.HasKey(e => e.Id);

            e.Property(e => e.Name).HasColumnType("varchar(255)").IsRequired();
            e.Property(e => e.Cost).HasColumnType("integer").IsRequired();
            e.Property(e => e.NbRound).HasColumnType("integer").IsRequired();

            e.HasOne(e => e.Skill)
                .WithMany()
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasData(
                  new Training { Id = 1, Name = "Formation HTML", Cost = 5000, NbRound = 2, SkillId = 1 },
                  new Training { Id = 2, Name = "Formation CSS", Cost = 5000, NbRound = 2, SkillId = 2 },
                  new Training { Id = 3, Name = "Formation JavaScript", Cost = 5000, NbRound = 2, SkillId = 3 },
                  new Training { Id = 4, Name = "Formation TypeScript", Cost = 5000, NbRound = 2, SkillId = 4 },
                  new Training { Id = 5, Name = "Formation React", Cost = 5000, NbRound = 2, SkillId = 5 },
                  new Training { Id = 6, Name = "Formation Angular", Cost = 5000, NbRound = 2, SkillId = 6 },
                  new Training { Id = 7, Name = "Formation Vue.js", Cost = 5000, NbRound = 2, SkillId = 7 },
                  new Training { Id = 8, Name = "Formation Node.js", Cost = 5000, NbRound = 2, SkillId = 8 },
                  new Training { Id = 9, Name = "Formation Express.js", Cost = 5000, NbRound = 2, SkillId = 9 },
                  new Training { Id = 10, Name = "Formation ASP.NET Core", Cost = 5000, NbRound = 2, SkillId = 10 },
                  new Training { Id = 11, Name = "Formation Ruby on Rails", Cost = 5000, NbRound = 2, SkillId = 11 },
                  new Training { Id = 12, Name = "Formation Django", Cost = 5000, NbRound = 2, SkillId = 12 },
                  new Training { Id = 13, Name = "Formation Flask", Cost = 5000, NbRound = 2, SkillId = 13 },
                  new Training { Id = 14, Name = "Formation PHP", Cost = 5000, NbRound = 2, SkillId = 14 },
                  new Training { Id = 15, Name = "Formation Laravel", Cost = 5000, NbRound = 2, SkillId = 15 },
                  new Training { Id = 16, Name = "Formation Spring Boot", Cost = 5000, NbRound = 2, SkillId = 16 },
                  new Training { Id = 17, Name = "Formation SQL", Cost = 5000, NbRound = 2, SkillId = 17 },
                  new Training { Id = 18, Name = "Formation NoSQL", Cost = 5000, NbRound = 2, SkillId = 18 },
                  new Training { Id = 19, Name = "Formation GraphQL", Cost = 5000, NbRound = 2, SkillId = 19 },
                  new Training { Id = 20, Name = "Formation REST APIs", Cost = 5000, NbRound = 2, SkillId = 20 }
               );

        });

        modelBuilder.Entity<InTraining>(e =>
        {
            e.ToTable("in_trainings");
            e.HasKey(e => new { e.EmployeeId, e.TrainingId , e.StartRoundId });


            e.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Training)
                .WithMany()
                .HasForeignKey(e => e.TrainingId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Round)
                .WithMany()
                .HasForeignKey(e => e.StartRoundId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
