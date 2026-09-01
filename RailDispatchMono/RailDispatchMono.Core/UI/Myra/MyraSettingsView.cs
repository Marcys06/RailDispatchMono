using System;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

internal sealed class MyraSettingsView
{
    public Widget Root { get; }

    public MyraSettingsView(string displayMode, string windowSize, string language, string particleEffect, string back, Action displayModeAction, Action windowSizeAction, Action languageAction, Action particleEffectAction, Action backAction)
    {
        var grid = new Grid
        {
            Width = 520,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10
        };

        for (int i = 0; i < 6; i++)
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        AddLabel(grid, 0, "USTAWIENIA");
        AddButton(grid, 1, displayMode, displayModeAction);
        AddButton(grid, 2, windowSize, windowSizeAction);
        AddButton(grid, 3, language, languageAction);
        AddButton(grid, 4, particleEffect, particleEffectAction);
        AddButton(grid, 5, back, backAction);
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
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        button.Click += (_, _) => action();
        Grid.SetRow(button, row);
        grid.Widgets.Add(button);
    }
}
