using System.Net.Http.Json;

using Terminal.Gui;

namespace Client.Screens;

public class StatisticScreen(Window target)
{

    private Window Target { get; } = target;
    private readonly MainMenuActionList StatList = new();
    private readonly Button ReturnedButton = new();
    private bool Returned = false;

    
    public async Task Show()
    {
        // Prepare the window (clear old content, set title, etc.)
        await BeforeShow();
        Console.WriteLine("StatisticScreen shown");
        await AddStatistics();
        Console.WriteLine("StatisticScreen after add stats");

        await WaitForReturn();
    }

    // This method resets the window before showing the menu.
    private Task BeforeShow()
    {
        Target.RemoveAll(); // remove all widgets/elements from window
        Target.Title = $"{MainWindow.Title} - [Statistics]"; // set window title
        return Task.CompletedTask; // nothing else to do, return "finished" task
    }

    private async Task WaitForReturn()
    {
        while (!Returned)
        {
            await Task.Delay(200);
        }
    }

    // Add the statistic text elements
    private async Task AddStatistics()
    {
        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        var stats = await httpClient.GetFromJsonAsync<Client.Records.StatisticsResult>("/statistics/global");
        // Create labels
        var totalGamesLabel = new Label()

        {
            X = 0,  // horizontal position: center of window
            Y = 0,  // vertical position: 2 rows down from top
            Width = 40,
            Text = $"Total Players : {stats?.TotalPlayers ?? 0}"
        };
        var totalPlayersLabel = new Label()
        {
            X = Pos.Left(totalGamesLabel),
            Y = Pos.Bottom(totalGamesLabel) + 1, // place below the first label
            Text = $"Total Game :{stats?.TotalGames ?? 0}"
        };

        ReturnedButton.X = Pos.Center();
        ReturnedButton.Y = Pos.Bottom(totalPlayersLabel);
        ReturnedButton.Text = "Exit";
        ReturnedButton.Accept += (_, __) => Returned = true;

        Target.Add(totalGamesLabel, totalPlayersLabel, ReturnedButton);

        await Task.CompletedTask;
    }

}
