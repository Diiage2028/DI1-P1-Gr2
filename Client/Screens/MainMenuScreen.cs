using System.Collections;
using System.Collections.Specialized;

using Terminal.Gui;

namespace Client.Screens;

// This class represents the "Main Menu" screen of the client application.
// It shows a menu with options (create game, join game, quit).
public class MainMenuScreen(Window target)
{
    // Reference to the "window" where the screen will be drawn.
    public Window Target { get; } = target;

    // The menu options (list of actions).
    private readonly MainMenuActionList ActionList = new();

    // The action chosen by the user. Starts as null (nothing chosen yet).
    public MainMenuActionList.Action? Action { get; private set; } = null;

    // Show() displays the screen, waits for the user to choose an option,
    // and then loads the next screen based on that choice.
    public async Task Show()
    {
        // Prepare the window (clear old content, set title, etc.)
        await BeforeShow();

        // Display the menu and wait until the user selects something.
        await SelectAction();

        Console.WriteLine("Main menu");
        // Based on the chosen action, decide which screen to show next.
        var next = Action switch
        {
            MainMenuActionList.Action.CREATE_GAME => new CreateGameScreen(Target).Show(),
            MainMenuActionList.Action.JOIN_GAME => new JoinGameScreen(Target).Show(),
            MainMenuActionList.Action.STATISTIC => new StatisticScreen(Target).Show(),
            MainMenuActionList.Action.QUIT => Task.Run(() => Application.RequestStop()), // closes app
            _ => Task.Run(() => Application.RequestStop()) // fallback = close app
        };

        // Execute the chosen action (wait until the next screen finishes).
        await next;
    }

    // This method resets the window before showing the menu.
    private Task BeforeShow()
    {
        Target.RemoveAll(); // remove all widgets/elements from window
        Target.Title = $"{MainWindow.Title} - [Main Menu]"; // set window title
        return Task.CompletedTask; // nothing else to do, return "finished" task
    }

    // This method displays the menu and waits until the user selects something.
    private async Task SelectAction()
    {
        // Position the menu in the center of the window
        ActionList.X = ActionList.Y = Pos.Center();
        ActionList.Width = 13;
        ActionList.Height = 4;

        // When a menu item is selected, save the action into "Action"
        ActionList.OpenSelectedItem += (_, selected) =>
        {
            Action = (MainMenuActionList.Action) selected.Value;
        };

        // Add the menu to the window so it is visible
        Target.Add(ActionList);

        // Put the cursor/focus on the menu and select the first item
        ActionList.SetFocus();
        ActionList.MoveHome();

        // Wait until the user chooses an option (Action will not be null anymore).
        while (Action is null)
        {
            await Task.Delay(100); // check every 100 ms
        }
        ;
    }
}

// This class represents the list of actions in the main menu (using Terminal.Gui's ListView).
public class MainMenuActionList : ListView
{
    // Enum with the possible menu actions
    public enum Action
    {
        CREATE_GAME,
        JOIN_GAME,
        STATISTIC,
        QUIT
    }

    // Data source that holds the actions to display
    private readonly MainMenuActionListDataSource Actions = [
        Action.CREATE_GAME,
        Action.JOIN_GAME,
        Action.STATISTIC,
        Action.QUIT
    ];

    // Constructor: tell ListView to use our data source
    public MainMenuActionList()
    {
        Source = Actions;
    }
}

// This class defines how the menu actions will be displayed in the UI.
public class MainMenuActionListDataSource : List<MainMenuActionList.Action>, IListDataSource
{
    // The number of items in the list
    public int Length => Count;

    // Required by IListDataSource, but not implemented here
    public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    // Event that would notify changes in the list (not used here)
    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    // Dispose method to clean resources (not much needed here)
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // We don't use "marking" (like checkbox selection), so always false
    public bool IsMarked(int item)
    {
        return false;
    }

    // This method tells the UI how to render (draw) each item in the list.
    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        switch (item)
        {
            case (int) MainMenuActionList.Action.CREATE_GAME:
                driver.AddStr("Create a game");
                break;
            case (int) MainMenuActionList.Action.JOIN_GAME:
                driver.AddStr("Join a game");
                break;
            case (int) MainMenuActionList.Action.STATISTIC:
                driver.AddStr("Statistics");
                break;
            case (int) MainMenuActionList.Action.QUIT:
                driver.AddStr("Quit");
                break;
        }
    }

    // Not used: marking items
    public void SetMark(int item, bool value) { }

    // Convert list to IList (for compatibility with the interface)
    public IList ToList()
    {
        return this;
    }
}
