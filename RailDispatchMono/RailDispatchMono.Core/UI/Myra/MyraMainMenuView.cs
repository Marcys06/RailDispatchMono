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

    public MyraMainMenuView(Action newGame, Action settings, Action about, Action quit)
    {
        var grid = new Grid
        {
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10,
            ColumnSpacing = 10
        };

        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var title = new Label
        {
            Text = "RAIL DISPATCHER",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(title, 0);
        grid.Widgets.Add(title);

        AddButton(grid, 1, "NOWA GRA", newGame);
        AddButton(grid, 2, "USTAWIENIA", settings);
        AddButton(grid, 3, "O GRZE", about);
        AddButton(grid, 4, "WYJDŹ", quit);

        Root = grid;
    }

    private static void AddButton(Grid grid, int row, string text, Action action)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        button.Click += (_, _) => action();
        Grid.SetRow(button, row);
        grid.Widgets.Add(button);
    }
}
