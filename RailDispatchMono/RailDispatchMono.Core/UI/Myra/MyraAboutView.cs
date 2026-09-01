using System;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

internal sealed class MyraAboutView
{
    public Widget Root { get; }

    public MyraAboutView(string builtWith, string website, string back, Action websiteAction, Action backAction)
    {
        var grid = new Grid
        {
            Width = 560,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10
        };

        for (int i = 0; i < 4; i++)
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        AddLabel(grid, 0, "O GRZE");
        AddLabel(grid, 1, builtWith);
        AddButton(grid, 2, website, websiteAction);
        AddButton(grid, 3, back, backAction);
        Root = grid;
    }

    private static void AddLabel(Grid grid, int row, string text)
    {
        var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
        Grid.SetRow(label, row);
        grid.Widgets.Add(label);
    }

    private static void AddButton(Grid grid, int row, string text, Action action)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 460,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        button.Click += (_, _) => action();
        Grid.SetRow(button, row);
        grid.Widgets.Add(button);
    }
}
