using FluentResults;

using Microsoft.EntityFrameworkCore;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Extensions;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WssDbContext>();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddSignalR();

builder.Services.AddTransient<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddTransient<IConsultantsRepository, ConsultantsRepository>();
builder.Services.AddTransient<IEmployeesRepository, EmployeesRepository>();
builder.Services.AddTransient<IGamesRepository, GamesRepository>();
builder.Services.AddTransient<IPlayersRepository, PlayersRepository>();
builder.Services.AddTransient<IRoundsRepository, RoundsRepository>();
builder.Services.AddTransient<ISkillsRepository, SkillsRepository>();
builder.Services.AddTransient<IGameStatsRepository, GameStatsRepository>();

builder.Services.AddTransient<IAction<ActInRoundParams, Result<Round>>, ActInRound>();
builder.Services.AddTransient<IAction<ApplyRoundActionParams, Result>, ApplyRoundAction>();
builder.Services.AddTransient<IAction<CreateCompanyParams, Result<Company>>, CreateCompany>();
builder.Services.AddTransient<IAction<CreateEmployeeParams, Result<Employee>>, CreateEmployee>();
builder.Services.AddTransient<IAction<CreateGameParams, Result<Game>>, CreateGame>();
builder.Services.AddTransient<IAction<CreatePlayerParams, Result<Player>>, CreatePlayer>();
builder.Services.AddTransient<IAction<FinishGameParams, Result<Game>>, FinishGame>();
builder.Services.AddTransient<IAction<FinishRoundParams, Result<Round>>, FinishRound>();
builder.Services.AddTransient<IAction<JoinGameParams, Result<Player>>, JoinGame>();
builder.Services.AddTransient<IAction<StartGameParams, Result<Game>>, StartGame>();
builder.Services.AddTransient<IAction<StartRoundParams, Result<Round>>, StartRound>();
builder.Services.AddTransient<IAction<GetStatsParams, Result<GameStat>>, GetStatsAction>();

builder.Services.AddTransient<IGameHubService, GameHubService>();
builder.Services.AddTransient<IMainHubService, MainHubService>();

var app = builder.Build();

// Apply database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WssDbContext>();
        context.Database.Migrate(); // This applies pending migrations
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
        // Don't throw here unless you want the application to fail completely
        // You might want to log this error instead
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

app.MapHub<MainHub>("/main");
app.MapHub<GameHub>("/games/{gameId}");

app.UseHttpsRedirection();

app.Run();
