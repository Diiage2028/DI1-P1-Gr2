using Client.Records;
using Terminal.Gui;

namespace Client.Screens;

public class FinishedGameScreen(Window target)
{
    private readonly Window Target = target;

    public void Show()
    {
        Target.RemoveAll();

        var resultText = new Label()
        {
            Text = "Game Finished",
            X = Pos.Center(),
            Y = 1
        };

        Target.Add(resultText);

        // Main menu button
        var menuButton = new Button()
        {
            Text = "Back to Main Menu",
            X = Pos.Center(),
            Y = Pos.Bottom(Target) + 3,
            Width = 20
        };

        // Make the event handler async
        menuButton.Accept += async (_, __) => await ReturnToMainMenu();
        Target.Add(menuButton);

        menuButton.SetFocus();
    }

    private async Task ReturnToMainMenu()
    {
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }
}
