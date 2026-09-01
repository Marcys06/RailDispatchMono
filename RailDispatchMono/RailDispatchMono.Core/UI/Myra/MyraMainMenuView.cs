using System;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>
/// Standard Myra representation of the main menu actions.
/// Action ownership remains in the screen; this class only builds widgets.
/// </summary>
internal sealed class MyraMainMenuView
{
    public Widget Root { get; }

    public MyraMainMenuView(Action newGame, Action loadGame, Action settings, Action about, Action quit)
    {
        var grid = new Grid
        {
            RowSpacing = 10,
            ColumnSpacing = 10
        };

        for (int i = 0; i < 6; i++)
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        grid.Widgets.Add(new Label { Text = "RAIL DISPATCHER" });
        AddButton(grid, 1, "NOWA GRA", newGame);
        AddButton(grid, 2, "WCZYTAJ GRĘ", loadGame);
        AddButton(grid, 3, "USTAWIENIA", settings);
        AddButton(grid, 4, "O GRZE", about);
        AddButton(grid, 5, "WYJDŹ", quit);

        Root = grid;
    }

    private static void AddButton(Grid grid, int row, string text, Action action)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 320
        };

        button.Click += (_, _) => action();
        Grid.SetRow(button, row);
        grid.Widgets.Add(button);
    }
}
