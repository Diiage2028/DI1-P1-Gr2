using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Net.Http.Json;

using Client.Records;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Terminal.Gui;

namespace Client.Screens;

public class CurrentGameScreen(Window target, int gameId, string playerName)
{
    private readonly Window Target = target;

    // Reference to currently displayed view
    private CurrentGameView? CurrentView = null;

    // Game metadata
    private readonly int GameId = gameId;
    private readonly string PlayerName = playerName;
    private GameOverview? CurrentGame = null;

    // Game state flags
    private bool CurrentGameLoading = true;
    private bool CurrentGameStarted = false;
    private bool CurrentGameEnded = false;

    // Change from single action to list of actions
    private List<CurrentGameActionList.Action>? CurrentRoundActions = null;

    public async Task Show()
    {
        await BeforeShow();
        await LoadGame();
        await DisplayMainView();
        await DisplayCompanyView();
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();

        ReloadWindowTitle();

        return Task.CompletedTask;
    }

    // Update title with the game name
    private void ReloadWindowTitle()
    {
        var gameName = CurrentGame is null ? "..." : CurrentGame.Name;
        Target.Title = $"{MainWindow.Title} - [Game {gameName}]";
    }

    private void RedirectToFinishedGame()
    {
        if (CurrentGame == null) return;

        Target.RemoveAll();
        var finishedScreen = new FinishedGameScreen(Target);
        finishedScreen.Show();
    }

    // Connects to SignalR hub to receive game updates
    private async Task LoadGame()
    {
        // Setup SignalR connection to game hub
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{WssConfig.WebSocketServerScheme}://{WssConfig.WebSocketServerDomain}:{WssConfig.WebSocketServerPort}/games/{GameId}"), opts =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    }

                    return message;
                };
            })
            .AddJsonProtocol()
            .Build();

        // Handle incoming game update events
        hubConnection.On<GameOverview>("CurrentGameUpdated", data =>
        {
            CurrentGame = data;
            ReloadWindowTitle();
            CurrentGameLoading = false;
            CurrentRoundActions = null;
            if (data.Status == "InProgress") { CurrentGameStarted = true; }
            if (data.Status == "Finished") { CurrentGameEnded = true; }
        });

        await hubConnection.StartAsync();

        var loadingDialog = new Dialog()
        {
            Width = 17,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Loading game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        while (CurrentGameLoading) { await Task.Delay(100); }

        Target.Remove(loadingDialog);

        if (CurrentGameEnded)
        {
            RedirectToFinishedGame();
        }
    }

    private async Task DisplayMainView()
    {
        Target.RemoveAll();

        var mainView = new CurrentGameMainView(CurrentGame!, PlayerName);

        CurrentView = mainView;

        mainView.X = mainView.Y = Pos.Center();
        mainView.OnStart = (_, __) => { CurrentGameStarted = true; };

        Target.Add(mainView);

        while (!CurrentGameStarted)
        {
            await Task.Delay(100);
        }
    }

    private async Task DisplayCompanyView()
    {
        Target.RemoveAll();

        var companyView = new CurrentGameCompanyView(CurrentGame!, PlayerName);

        CurrentView = companyView;

        companyView.X = companyView.Y = 5;
        companyView.Width = companyView.Height = Dim.Fill() - 5;

        // Update to handle list of actions
        companyView.OnRoundActions = (_, roundActions) => { CurrentRoundActions = roundActions; };

        Target.Add(companyView);

        // Wait until player confirms their turn with actions
        while (CurrentRoundActions is null && !CurrentGameEnded)
        {
            await Task.Delay(100);
        }

        var lastRound = CurrentGame!.CurrentRound;

        if (CurrentRoundActions != null)
        {
            await ActInRound();
        }

        while (
            !CurrentGameEnded &&
            CurrentGame!.CurrentRound == lastRound
        )
        {
            await Task.Delay(100);
        }

        // Reset for next round
        CurrentRoundActions = null;

        if (!CurrentGameEnded)
        {
            await DisplayCompanyView();
        }
    }

     private async Task ActInRound()
    {
        Target.RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 25,
            Height = 3,
            Title = "Processing Actions"
        };

        var loadingText = new Label()
        {
            Text = "Processing actions...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);
        Target.Add(loadingDialog);

        try
        {
            using var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };

            using var httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
            };

            var successfulActions = new List<CurrentGameActionList.Action>();
            var failedActions = new List<CurrentGameActionList.Action>();

            // Send all actions in sequence
            foreach (var action in CurrentRoundActions!)
            {
                if (action == CurrentGameActionList.Action.ConfirmRound)
                {
                    continue;
                }

                try
                {
                    loadingText.Text = $"Processing: {action}...";

                    var roundId = CurrentGame!.Rounds.MaxBy(r => r.Id)!.Id;
                    var playerId = CurrentGame.Players.First(p => p.Name == PlayerName).Id;

                    var request = new HttpRequestMessage(HttpMethod.Post, $"/rounds/{roundId}/act")
                    {
                        Content = JsonContent.Create(new
                        {
                            ActionType = action.ToString(),
                            ActionPayload = "{}",
                            PlayerId = playerId
                        })
                    };

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        successfulActions.Add(action);
                    }
                    else
                    {
                        failedActions.Add(action);
                        ShowErrorDialog($"Failed to execute: {action}\nStatus: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    failedActions.Add(action);
                    ShowErrorDialog($"Error executing {action}:\n{ex.Message}");
                }
            }

            // Show summary
            if (failedActions.Count == 0)
            {
                loadingText.Text = "All actions completed successfully!";
                await Task.Delay(1000);
            }
            else
            {
                loadingText.Text = $"{successfulActions.Count} successful, {failedActions.Count} failed";
                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"Network error: {ex.Message}");
        }
        finally
        {
            Target.Remove(loadingDialog);
        }
    }

    private void ShowErrorDialog(string message)
    {
        var errorDialog = new Dialog()
        {
            Width = 200,
            Height = 6,
            Title = "Error"
        };

        var errorText = new Label()
        {
            Text = message,
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2
        };

        // Correct button creation - no parameters in constructor
        var okButton = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(errorText),
            Text = "OK"  // Set text as property, not in constructor
        };

        errorDialog.Add(errorText, okButton);

        Application.Run(errorDialog);
    }
}

public abstract class CurrentGameView : View
{
    public abstract Task Refresh(GameOverview game);
}

public class CurrentGameMainView : CurrentGameView
{
    private GameOverview Game;
    private readonly string PlayerName;

    private FrameView? Players;
    private FrameView? Status;
    private Button? StartButton;

    public EventHandler<HandledEventArgs> OnStart = (_, __) => { };

    public CurrentGameMainView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;

        Width = Dim.Auto();
        Height = Dim.Auto();

        SetupPlayers();
        SetupStatus();
        SetupStartButton();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;

        Remove(Players);
        Remove(Status);
        Remove(StartButton);

        SetupPlayers();
        SetupStatus();
        SetupStartButton();

        return Task.CompletedTask;
    }

    private void SetupPlayers()
    {
        Players = new()
        {
            Title = $"Players ({Game.PlayersCount}/{Game.MaximumPlayersCount})",
            X = 0,
            Y = 0,
            Width = 100,
            Height = 6 + Game.Players.Count,
            Enabled = false
        };

        Add(Players);

        var dataTable = new DataTable();

        dataTable.Columns.Add("Name");
        dataTable.Columns.Add("Company");
        dataTable.Columns.Add("Treasury");
        dataTable.Columns.Add("⭐");

        foreach (var player in Game.Players.ToList())
        {
            dataTable.Rows.Add([
                player.Name,
                player.Company.Name,
                $"{player.Company.Treasury} $",
                PlayerName == player.Name ? "⭐" : ""
            ]);
        }

        var dataTableSource = new DataTableSource(dataTable);

        var tableView = new TableView()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Game.Players.Max(p => p.Name.Length)
                + Game.Players.Max(p => p.Company.Name.Length)
                + Game.Players.Max(p => p.Company.Treasury.ToString().Length)
                + " $".Length
                + "⭐".Length
                + 6,
            Height = Dim.Fill(),
            Table = dataTableSource,
            Style = new TableStyle
            {
                ShowHorizontalBottomline = true,
                ExpandLastColumn = false,
            }
        };

        Players.Add(tableView);
    }

    private void SetupStatus()
    {
        Status = new()
        {
            Title = "Status",
            X = Pos.Left(Players!),
            Y = Pos.Bottom(Players!) + 2,
            Width = Players!.Width,
            Height = 3
        };

        Add(Status);

        var statusLabel = new Label() { Text = Game.Status is null ? "" : Game.Status, X = Pos.Center(), Y = Pos.Center() };

        Status.Add(statusLabel);
    }

    private void SetupStartButton()
    {
        if (PlayerName != Game.Players.First().Name) { return; }

        StartButton = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(Status!) + 2,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = Dim.Auto(DimAutoStyle.Text),
            Text = "Start Game",
        };

        StartButton.Accept += async (_, __) => await StartGame();

        Add(StartButton);

        StartButton.SetFocus();
    }

    private async Task StartGame()
    {
        RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Starting game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Add(loadingDialog);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        var request = httpClient.PostAsJsonAsync($"/games/{Game.Id}/start", new { });
        var response = await request;

        if (!response.IsSuccessStatusCode)
        {
            await Refresh(Game);
        }
        else
        {
            OnStart(null, new HandledEventArgs());
        }
    }
}

public class CurrentGameCompanyView : CurrentGameView
{
    private GameOverview Game;
    private PlayerOverview CurrentPlayer;
    private readonly string PlayerName;
    private List<CurrentGameActionList.Action> CurrentRoundActions = new();
    public EventHandler<List<CurrentGameActionList.Action>> OnRoundActions = (_, __) => { };

    private View? Header;
    private View? Body;
    private View? LeftBody;
    private View? RightBody;

    private FrameView? Player;
    private FrameView? Company;
    private FrameView? Treasury;
    private FrameView? Rounds;
    private FrameView? Employees;
    private FrameView? Projects;
    private FrameView? Formation;
    private FrameView? Actions;

    public CurrentGameCompanyView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        Width = Dim.Auto(DimAutoStyle.Auto);
        Height = Dim.Auto(DimAutoStyle.Auto);

        SetupHeader();
        SetupBody();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        RemoveHeader();
        RemoveBody();

        SetupHeader();
        SetupBody();

        return Task.CompletedTask;
    }

    private void SetupHeader()
    {
        Header = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Auto(DimAutoStyle.Content)
        };

        SetupPlayer();
        SetupCompany();
        SetupTreasury();
        SetupRounds();

        Add(Header);
    }

    private void SetupBody()
    {
        Body = new()
        {
            X = 0,
            Y = Pos.Bottom(Header!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        SetupLeftBody();
        SetupRightBody();

        Add(Body);
    }

    private void SetupLeftBody()
    {
        LeftBody = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(80),
            Height = Dim.Fill()
        };

        SetupEmployees();
        SetupProjects();
        SetupFormation();

        Body!.Add(LeftBody);
    }

    private void SetupRightBody()
    {
        RightBody = new()
        {
            X = Pos.Right(LeftBody!),
            Y = Pos.Top(LeftBody!),
            Width = Dim.Percent(20),
            Height = Dim.Fill()
        };

        SetupActions();

        Body!.Add(RightBody);
    }

    private void SetupPlayer()
    {
        Player = new()
        {
            Title = "Player",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = 0,
            Y = 0
        };

        Player.Add(new Label { Text = CurrentPlayer.Name });

        Header!.Add(Player);
    }

    private void SetupCompany()
    {
        Company = new()
        {
            Title = "Company",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Player!),
            Y = 0
        };

        Company.Add(new Label { Text = CurrentPlayer.Company.Name });

        Header!.Add(Company);
    }

    private void SetupTreasury()
    {
        Treasury = new()
        {
            Title = "Treasury",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Company!),
            Y = 0
        };

        Treasury.Add(new Label { Text = $"{CurrentPlayer.Company.Treasury} $" });

        Header!.Add(Treasury);
    }

    private void SetupRounds()
    {
        Rounds = new()
        {
            Title = "Rounds",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Treasury!),
            Y = 0
        };

        Rounds.Add(new Label { Text = $"{Game.CurrentRound}/{Game.MaximumRounds}" });

        Header!.Add(Rounds);
    }

    private void SetupEmployees()
    {
        Employees = new()
        {
            Title = "Employees",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40)
        };

        var employeesTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var employeesData = new List<TreeNode>();

        foreach (var employee in CurrentPlayer.Company.Employees.ToList())
        {
            var node = new TreeNode($"{employee.Name} | {employee.Salary} $");
            var skills = employee.Skills.ToList();

            foreach (var skill in skills)
            {
                node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}"));
            }

            employeesData.Add(node);
        }

        employeesTree.BorderStyle = LineStyle.None;
        employeesTree.AddObjects(employeesData);
        employeesTree.ExpandAll();

        Employees.Add(employeesTree);

        LeftBody!.Add(Employees);
    }

    private void SetupProjects()
    {
        Projects = new()
        {
            Title = "Projects",
            X = Pos.Left(Employees!),
            Y = Pos.Bottom(Employees!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        var projectsTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var projectsData = new List<TreeNode>();

        // if (Game.Projects != null)
        // {
        //     foreach (var project in Game.Projects.ToList())
        //     {
        //         var node = new TreeNode($"{project.Name} | Reward: {project.Reward} $ | Deadline: {project.Deadline}");
        //         var requirements = project.Requirements.ToList();
        //
        //         foreach (var requirement in requirements)
        //         {
        //             node.Children.Add(new TreeNode($"{requirement.SkillName} | Level: {requirement.RequiredLevel}"));
        //         }
        //
        //         projectsData.Add(node);
        //     }
        // }

        projectsTree.BorderStyle = LineStyle.None;
        projectsTree.AddObjects(projectsData);
        projectsTree.ExpandAll();

        Projects.Add(projectsTree);

        LeftBody!.Add(Projects);
    }

    private void SetupFormation()
    {
        Formation = new()
        {
            Title = "Formation",
            X = Pos.Left(Projects!),
            Y = Pos.Bottom(Projects!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        var formationTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var formationData = new List<TreeNode>();

        // if (Game.FormationPrograms != null)
        // {
        //     foreach (var program in Game.FormationPrograms.ToList())
        //     {
        //         var node = new TreeNode($"{program.Name} | Cost: {program.Cost} $ | Duration: {program.Duration}");
        //         var skills = program.Skills.ToList();
        //
        //         foreach (var skill in skills)
        //         {
        //             node.Children.Add(new TreeNode($"{skill.Name} | Level Increase: +{skill.LevelIncrease}"));
        //         }
        //
        //         formationData.Add(node);
        //     }
        // }

        formationTree.BorderStyle = LineStyle.None;
        formationTree.AddObjects(formationData);
        formationTree.ExpandAll();

        Formation.Add(formationTree);

        LeftBody!.Add(Formation);
    }

    private void SetupActions()
    {
        Actions = new()
        {
            Title = "Actions",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var actionList = new CurrentGameActionList()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        actionList.OpenSelectedItem += (_, selected) =>
        {
            var action = (CurrentGameActionList.Action)selected.Value;

            if (action == CurrentGameActionList.Action.ConfirmRound)
            {
                // When confirming, send all accumulated actions
                OnRoundActions(null, new List<CurrentGameActionList.Action>(CurrentRoundActions));
            }
            else
            {
                // Add action to current turn (you might want to show feedback)
                CurrentRoundActions.Add(action);

                // Show current actions in a status area
                UpdateActionsStatus();
            }
        };

        Actions.Add(actionList);

        // Add a status view to show current actions
        var statusView = new Label()
        {
            Text = "Current actions: None",
            X = 0,
            Y = Pos.Bottom(actionList),
            Width = Dim.Fill(),
            Height = 1
        };

        Actions.Add(statusView);

        actionList.SetFocus();
        actionList.MoveHome();

        RightBody!.Add(Actions);
    }

    private void UpdateActionsStatus()
    {
        var statusLabel = Actions!.Subviews.OfType<Label>().FirstOrDefault();
        if (statusLabel != null)
        {
            if (CurrentRoundActions.Count == 0)
            {
                statusLabel.Text = "Current actions: None";
            }
            else
            {
                var actionsText = string.Join(", ", CurrentRoundActions.Select(a => a.ToString()));
                statusLabel.Text = $"Current actions: {actionsText}";
            }
        }
    }

    private void RemoveHeader()
    {
        Header!.Remove(Player);
        Header!.Remove(Company);
        Header!.Remove(Treasury);
        Header!.Remove(Rounds);

        Remove(Header);
    }

    private void RemoveBody()
    {
        LeftBody!.Remove(Employees);
        LeftBody!.Remove(Projects);
        LeftBody!.Remove(Formation);

        RightBody!.Remove(Actions);

        Body!.Remove(LeftBody);
        Body!.Remove(RightBody);

        Remove(Body);
    }
}

public class CurrentGameActionList : ListView
{
    public enum Action
    {
        SendEmployeeForTraining,
        ParticipateInProject,
        EnrollEmployee,
        FireAnEmployee,
        ConfirmRound
    }

    private readonly CurrentGameActionListDataSource Actions = [
        Action.SendEmployeeForTraining,
        Action.ParticipateInProject,
        Action.EnrollEmployee,
        Action.FireAnEmployee,
        Action.ConfirmRound
    ];

    public CurrentGameActionList()
    {
        Source = Actions;
    }
}

public class CurrentGameActionListDataSource : List<CurrentGameActionList.Action>, IListDataSource
{
    public int Length => Count;

    public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool IsMarked(int item)
    {
        return false;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        var actionText = this[item] switch
        {
            CurrentGameActionList.Action.SendEmployeeForTraining => "Send Employee For Training",
            CurrentGameActionList.Action.ParticipateInProject => "Participate In Project",
            CurrentGameActionList.Action.EnrollEmployee => "Enroll Employee",
            CurrentGameActionList.Action.FireAnEmployee => "Fire An Employee",
            CurrentGameActionList.Action.ConfirmRound => "✅ Confirm My Turn",
            _ => "Unknown Action"
        };

        driver.AddStr(actionText);
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}
