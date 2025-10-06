using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

using Terminal.Gui;

namespace Client.Screens;

public class CreateGameScreen
{
    private Window Target { get; }
    private CreateGameForm Form;
    private int? GameId = null;
    private bool Submitted = false;
    private bool Returned = false;
    private bool Errored = false;

    public CreateGameScreen(Window target)
    {
        Target = target;
        Form = new CreateGameForm();
    }

    public async Task Show()
    {
        await BeforeShow();

        //variable to control the retry loop
        bool shouldRetry;

        // Attempt to show the form and create the game, retrying if there are errors
        do
        {
            //reset the retry control variable
            shouldRetry = false;
            await DisplayForm();

            // if user chose to return, go back to main menu
            if (Returned)
            {
                await Return();
                return;
            }

            // if user submitted the form, try to create the game
            if (Submitted)
            {
                await CreateGame();

                if (Errored)
                {
                    // if there was an error, set the retry flag to true to show the form again
                    shouldRetry = true;
                    ResetFlags();
                }
            }

            // Repeat the loop if shouldRetry is true
        } while (shouldRetry);

        // if game creation was successful, proceed to the current game screen
        var currentGameScreen = new CurrentGameScreen(
            Target,
            GameId!.Value,
            Form.PlayerNameField.Text.ToString()!
        );
        // Show the current game screen
        await currentGameScreen.Show();
    }

    // Reset the control flags for form submission and error handling
    private void ResetFlags()
    {
        Submitted = false;
        Returned = false;
        Errored = false;
    }

    // Prepare the screen before showing it
    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = "Game - [Create Game]";
        return Task.CompletedTask;
    }

    // Handle returning to the main menu
    private async Task Return()
    {
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }

    // Display the form and wait for user interaction
    // handles both submission and return actions
    private async Task DisplayForm()
    {
        Form.OnReturn = (_, __) => { Returned = true; };
        Form.OnSubmit = (_, __) => { Submitted = true; };

        Form.FormView.X = Form.FormView.Y = Pos.Center();
        Form.FormView.Width = 70;
        Form.FormView.Height = 26;

        Form.ErrorMessage.Visible = true;

        Target.Add(Form.FormView);

        // continue waiting until the user either submits the form or chooses to return
        while (!Returned && !Submitted)
        {
            // brief delay to prevent busy-waiting
            await Task.Delay(100);
        }

        // remove the form from the screen after an action is taken
        Target.Remove(Form.FormView);
    }

    // Create a new game by sending a request to the server
    private async Task CreateGame()
    {
        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Creating game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);
        Target.Add(loadingDialog);

<<<<<<< HEAD
        try
=======
        // Security breach / ! \
        var httpHandler = new HttpClientHandler
>>>>>>> 6e737187807dcf3e29d970e7cf9d80ccc63edbd0
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            // we use 'using var' to ensure the HttpClient is disposed of properly
            using var httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
            };

            var gameName = Form.GameNameField.Text.ToString();
            var playerName = Form.PlayerNameField.Text.ToString();
            var companyName = Form.CompanyNameField.Text.ToString();

            // client-side validation of the rounds number
            // we convert the text to an integer and check if it's within the valid range
            if (!int.TryParse(Form.RoundsField.Text.ToString(), out int rounds))
            {
                Form.ErrorMessage.Text = "Rounds must be a valid number";
                Errored = true;
                return;
            }

            var requestBody = new { gameName, playerName, companyName, rounds };
            var response = await httpClient.PostAsJsonAsync("/games", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                Errored = true;
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // try to parse the error content as a list of strings (validation errors)
                    var errors = JsonSerializer.Deserialize<List<string>>(errorContent);
                    Form.ErrorMessage.Text = string.Join("\n", errors ?? new List<string> { "Unknown error" });
                }
                catch
                {
                    // if parsing fails, just display the raw error content
                    Form.ErrorMessage.Text = errorContent;
                }
            }
            else
            {
                // on success, parse the response to get the new game's ID
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                GameId = content.GetProperty("id").GetInt32();
            }
        }
        // ensure the loading dialog (creating game) is removed from the screen even if an error occurs
        finally
        {
            Target.Remove(loadingDialog);
        }
    }
}

public class CreateGameForm
{
    private EventHandler<HandledEventArgs> _onSubmit = (_, __) => { };
    private EventHandler<HandledEventArgs> _onReturn = (_, __) => { };

    public EventHandler<HandledEventArgs> OnSubmit
    {
        get => _onSubmit;
        set
        {
            SubmitButton.Accept -= _onSubmit;
            SubmitButton.Accept += value;
            _onSubmit = value;
        }
    }

    public EventHandler<HandledEventArgs> OnReturn
    {
        get => _onReturn;
        set
        {
            ReturnButton.Accept -= _onReturn;
            ReturnButton.Accept += value;
            _onReturn = value;
        }
    }

    public View FormView { get; }
    public View ButtonsView { get; }
    public Button SubmitButton { get; }
    public Button ReturnButton { get; }
    public Label GameNameLabel { get; }
    public Label PlayerNameLabel { get; }
    public Label CompanyNameLabel { get; }
    public Label RoundsLabel { get; }
    public Label ErrorMessage { get; }
    public TextField GameNameField { get; }
    public TextField PlayerNameField { get; }
    public TextField CompanyNameField { get; }
    public TextField RoundsField { get; }

    public CreateGameForm()
    {
        GameNameLabel = new Label()
        {
            X = 0,
            Y = 0,
            Width = 20,
            Text = "Game name :"
        };

        PlayerNameLabel = new Label()
        {
            X = Pos.Left(GameNameLabel),
            Y = Pos.Bottom(GameNameLabel) + 1,
            Width = 20,
            Text = "Player name :"
        };

        CompanyNameLabel = new Label()
        {
            X = Pos.Left(PlayerNameLabel),
            Y = Pos.Bottom(PlayerNameLabel) + 1,
            Width = 20,
            Text = "Company name :"
        };

        RoundsLabel = new Label()
        {
            X = Pos.Left(CompanyNameLabel),
            Y = Pos.Bottom(CompanyNameLabel) + 1,
            Width = 20,
            Text = "Rounds (5 - 20) :"
        };

        GameNameField = new TextField()
        {
            X = Pos.Right(GameNameLabel),
            Y = Pos.Top(GameNameLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        PlayerNameField = new TextField()
        {
            X = Pos.Right(PlayerNameLabel),
            Y = Pos.Top(PlayerNameLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        CompanyNameField = new TextField()
        {
            X = Pos.Right(CompanyNameLabel),
            Y = Pos.Top(CompanyNameLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        RoundsField = new TextField()
        {
            X = Pos.Right(RoundsLabel),
            Y = Pos.Top(RoundsLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        ButtonsView = new View()
        {
            Width = 1,
            Height = 1,
            X = Pos.Center(),
            Y = Pos.Bottom(RoundsLabel) + 1
        };

        SubmitButton = new Button()
        {
            Text = "Submit",
            IsDefault = true,
            X = Pos.Center(),
        };

        ErrorMessage = new Label()
        {
            Text = "",
            X = Pos.Center(),
            Y = Pos.Bottom(RoundsField) + 2,
            Width = Dim.Fill()
        };

        ReturnButton = new Button()
        {
            Text = "Return",
            IsDefault = false,
            X = Pos.Right(SubmitButton) + 1
        };

        ButtonsView.Add(SubmitButton, ReturnButton);

        var submitButtonWidth = SubmitButton.Text.Length + 4;
        var returnButtonWidth = ReturnButton.Text.Length + 4;
        ButtonsView.Width = submitButtonWidth + returnButtonWidth + 15;

        FormView = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        FormView.Add(
            GameNameLabel, PlayerNameLabel, CompanyNameLabel, RoundsLabel,
            GameNameField, PlayerNameField, CompanyNameField, RoundsField,
            ButtonsView, ErrorMessage
        );

        // Initialize event handlers to no-op to avoid null references
        OnSubmit = (_, __) => { };
        OnReturn = (_, __) => { };
    }
}
