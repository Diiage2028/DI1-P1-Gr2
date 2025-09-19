using System.Net.Http.Json;
using Client.Records;
using Terminal.Gui;

namespace Client.Screens;

public class StatsScreen(Window target)
{
    private Window Target { get; } = target;
    private readonly Button _returnButton = new() { Text = "Return" };

    private bool _returnRequested;
    private GameStat? _gameStat;
    private string? _errorMessage;

    public async Task Show()
    {
        await BeforeShow();
        await LoadStats();
        await DisplayStats();

        if (_returnRequested)
        {
            await ReturnToMainMenu();
        }
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Statistics]";
        return Task.CompletedTask;
    }

    private async Task ReturnToMainMenu()
    {
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }

    private async Task LoadStats()
    {
        try
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(httpHandler);
            httpClient.BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}");

            var response = await httpClient.GetAsync("/stats");

            if (response.IsSuccessStatusCode)
            {
                var stat = await response.Content.ReadFromJsonAsync<GameStat>();
                _gameStat = stat;
                _errorMessage = null;
            }
            else
            {
                await response.Content.ReadAsStringAsync();
                _errorMessage = "Impossible de récupérer les statistiques.";
                _gameStat = null;
            }
        }
        catch (Exception ex)
        {
            _gameStat = null;
            _errorMessage = $"Erreur: {ex.Message}";
        }
    }

    private async Task DisplayStats()
    {
        Target.RemoveAll();

        if (_gameStat == null)
        {
            var noStatsLabel = new Label()
            {
                Text = _errorMessage ?? "No statistics available",
                X = Pos.Center(),
                Y = Pos.Center() - 1
            };
            Target.Add(noStatsLabel);
        }
        else
        {
            // shwo stats if available with simple labels
            var titleLabel = new Label()
            {
                Text = "STATISTIQUES",
                X = Pos.Center(),
                Y = 2
            };
            Target.Add(titleLabel);

            var gamesLabel = new Label()
            {
                Text = $"Total parties: {_gameStat.GameCount}",
                X = Pos.Center(),
                Y = 4
            };
            Target.Add(gamesLabel);

            var playersLabel = new Label()
            {
                Text = $"Total joueurs: {_gameStat.PlayerCount}",
                X = Pos.Center(),
                Y = 5
            };
            Target.Add(playersLabel);
        }

        _returnButton.X = Pos.Center();
        _returnButton.Y = _gameStat == null ? Pos.Center() + 2 : 8;
        _returnButton.Accept += (_, _) => _returnRequested = true;

        Target.Add(_returnButton);
        _returnButton.SetFocus();

        // wait for return request to main menu
        while (!_returnRequested)
        {
            await Task.Delay(100);
        }
    }
}
