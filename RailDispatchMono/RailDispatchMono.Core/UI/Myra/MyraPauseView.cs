using System;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>
/// Myra presentation for the gameplay pause menu.
/// Gameplay state and persistence actions remain owned by PauseScreen.
/// </summary>
internal sealed class MyraPauseView
{
    public Widget Root { get; }

    public MyraPauseView(
        Action resume,
        Action save,
        Action load,
        Action quit,
        bool canLoad)
    {
        var grid = new Grid
        {
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10
        };

        for (int i = 0; i < 5; i++)
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var title = new Label
        {
            Text = "PAUZA",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(title, 0);
        grid.Widgets.Add(title);

        AddButton(grid, 1, "WZNÓW GRĘ", resume, true);
        AddButton(grid, 2, "ZAPISZ GRĘ", save, true);
        AddButton(grid, 3, "WCZYTAJ GRĘ", load, canLoad);
        AddButton(grid, 4, "WYJDŹ", quit, true);

        Root = grid;
    }

    private static void AddButton(Grid grid, int row, string text, Action action, bool enabled)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Center,
            Enabled = enabled   
        };

        button.Click += (_, _) => action();
        Grid.SetRow(button, row);
        grid.Widgets.Add(button);
    }
}
