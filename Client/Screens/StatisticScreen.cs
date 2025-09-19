using System.Text.Json;

using Terminal.Gui;

namespace Client.Screens;

public class StatisticScreen(Window target)
{

    private Window Target { get; } = target;
    private readonly MainMenuActionList StatList = new();
    private readonly Button ReturnedButton = new ();
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
            await Task.Delay(1000);
        }
    }

    // Add the statistic text elements
    private async Task AddStatistics()
    {
        Console.WriteLine("ENter add stat");
        // Create labels
        var totalGamesLabel = new Label()

        {
            X = 0,  // horizontal position: center of window
            Y = 0,              // vertical position: 2 rows down from top
            //Width = auto,
            Text = "Total Games :"
        };
        var totalPlayersLabel = new Label()
        {
            X = Pos.Left(totalGamesLabel),
            Y = Pos.Bottom(totalGamesLabel) + 1, // place below the first label
            Text = "Total Players :"
        };

        ReturnedButton.X = Pos.Center();
        ReturnedButton.Y = Pos.Bottom(totalPlayersLabel);
        ReturnedButton.Text = "Exit";
        ReturnedButton.Accept += (_, __) => Returned = true;

        Target.Add(totalGamesLabel, totalPlayersLabel, ReturnedButton);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        // Fetch data from Games API
        try
        {
            // GET total games
            var gamesResponse = await httpClient.GetAsync("/statistics/games/count");
            if (gamesResponse.IsSuccessStatusCode)
            {
                // Get the value inside the json
                var gamesJson = await gamesResponse.Content.ReadAsStringAsync();
                var gamesData = JsonSerializer.Deserialize<Dictionary<string, int>>(gamesJson);
                totalGamesLabel.Text = $"Total Games: {gamesData?["totalGames"] ?? 0}";
            }
            else
            {
                totalGamesLabel.Text = "Total Games: Error";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting fetching games statistics {ex.Message}");
            totalGamesLabel.Text = "Total Games: Error";
        }

        // Fetch data from Players API
        try
        {
            // GET total players
            var playersResponse = await httpClient.GetAsync("/statistics/players/count");
            if (playersResponse.IsSuccessStatusCode)
            {
                // Get the value inside the json
                var playersJson = await playersResponse.Content.ReadAsStringAsync();
                var playersData = JsonSerializer.Deserialize<Dictionary<string, int>>(playersJson);
                totalPlayersLabel.Text = $"Total Players: {playersData?["totalPlayers"] ?? 0}";
            }
            else
            {
                totalPlayersLabel.Text = "Total Players: Error";
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error getting fetching players statistics {ex.Message}");
            totalPlayersLabel.Text = "Total Players: Error";
        }
        


        await Task.CompletedTask;
    }
}
