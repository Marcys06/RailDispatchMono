using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.RollingStock;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.Screens;

namespace RailDispatchMono.Core.UI.Myra;

internal sealed class MyraDepotView
{
    public Widget Root { get; }
    private readonly DepotScreen _screen;
    private readonly Grid _compositionList;
    private readonly Label _locoDetails;
    private readonly Label _summary;
    private readonly Label _status;
    private readonly Button _createButton;

    public MyraDepotView(DepotScreen screen)
    {
        _screen = screen;
        var root = new VerticalStackPanel { Width = 1180, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Spacing = 8 };
        root.Widgets.Add(new Label { Text = "KREATOR POCIĄGU — DEPOT", HorizontalAlignment = HorizontalAlignment.Center });
        var columns = new Grid { Width = 1180, Height = 390, ColumnSpacing = 10 };
        columns.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        columns.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.15f));
        columns.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));

        var locoPanel = new VerticalStackPanel { Width = 370, Spacing = 4 };
        locoPanel.Widgets.Add(new Label { Text = "LOKOMOTYWY" });
        _locoDetails = new Label { Text = "Wybierz lokomotywę." };
        locoPanel.Widgets.Add(_locoDetails);
        foreach (var definition in RollingStockCatalog.Locomotives)
        {
            var button = new Button { Content = new Label { Text = $"{definition.DisplayName} • {definition.MaxSpeedKmh:0} km/h • {definition.MassTons:0} t • {definition.PowerMW:0.0} MW" }, Width = 370 };
            button.Click += (_, _) => _screen.SelectLocomotive(definition);
            locoPanel.Widgets.Add(button);
        }
        Grid.SetColumn(locoPanel, 0);
        columns.Widgets.Add(locoPanel);

        var compositionPanel = new VerticalStackPanel { Width = 420, Spacing = 4 };
        compositionPanel.Widgets.Add(new Label { Text = "SKŁAD" });
        _compositionList = new Grid { Width = 420 };
        compositionPanel.Widgets.Add(_compositionList);
        Grid.SetColumn(compositionPanel, 1);
        columns.Widgets.Add(compositionPanel);

        var wagonPanel = new VerticalStackPanel { Width = 370, Spacing = 4 };
        wagonPanel.Widgets.Add(new Label { Text = "WAGONY — kliknij, aby dodać" });
        foreach (var definition in RollingStockCatalog.Wagons)
        {
            var button = new Button { Content = new Label { Text = $"{definition.DisplayName} [{definition.ShortName}] • {definition.MassTons:0} t • {definition.MaxSpeedKmh:0} km/h" }, Width = 370 };
            button.Click += (_, _) => _screen.AddWagon(definition);
            wagonPanel.Widgets.Add(button);
        }
        Grid.SetColumn(wagonPanel, 2);
        columns.Widgets.Add(wagonPanel);
        root.Widgets.Add(columns);

        _summary = new Label { Text = string.Empty };
        root.Widgets.Add(_summary);
        _status = new Label { Text = string.Empty };
        root.Widgets.Add(_status);

        var actions = new HorizontalStackPanel { HorizontalAlignment = HorizontalAlignment.Center, Spacing = 8 };
        _createButton = new Button { Content = new Label { Text = "UTWÓRZ POCIĄG" }, Width = 190 };
        _createButton.Click += (_, _) => _screen.CreateTrain();
        actions.Widgets.Add(_createButton);
        var clearButton = new Button { Content = new Label { Text = "WYCZYŚĆ WAGONY" }, Width = 170 };
        clearButton.Click += (_, _) => _screen.ClearWagons();
        actions.Widgets.Add(clearButton);
        var cancelButton = new Button { Content = new Label { Text = "ANULUJ" }, Width = 130 };
        cancelButton.Click += (_, _) => _screen.Close();
        actions.Widgets.Add(cancelButton);
        root.Widgets.Add(actions);
        Root = root;
        Refresh();
    }

    public void Refresh()
    {
        var loco = _screen.SelectedLocomotive;
        _locoDetails.Text = loco == null
            ? "Wybierz lokomotywę."
            : $"{loco.DisplayName}\nVmax: {loco.MaxSpeedKmh:0} km/h\nMoc: {loco.PowerMW:0.0} MW\nMasa: {loco.MassTons:0.0} t\nDługość: {loco.LengthMeters:0.0} m\nPrzysp.: {loco.AccelerationMps2:0.00} m/s²\nHamowanie: {loco.DecelerationMps2:0.00} m/s²";
        _compositionList.Widgets.Clear();
        _compositionList.RowsProportions.Clear();
        for (int i = 0; i < _screen.Composition.Vehicles.Count; i++)
        {
            int index = i;
            var vehicle = _screen.Composition.Vehicles[i];
            var row = new HorizontalStackPanel { Spacing = 4 };
            row.Widgets.Add(new Label { Text = $"{i + 1}. {(vehicle is Locomotive ? loco?.ShortName ?? "Lokomotywa" : (vehicle as Wagon)?.ShortName ?? "Wagon")}", Width = 300 });
            if (vehicle is Wagon)
            {
                var remove = new Button { Content = new Label { Text = "USUŃ" }, Width = 80 };
                remove.Click += (_, _) => _screen.RemoveWagon(index);
                row.Widgets.Add(remove);
            }
            Grid.SetRow(row, _compositionList.Widgets.Count);
            _compositionList.RowsProportions.Add(new Proportion(ProportionType.Auto));
            _compositionList.Widgets.Add(row);
        }
        _summary.Text = $"Vmax składu: {_screen.Composition.EffectiveMaxSpeedKmh:0.0} km/h • Masa: {_screen.Composition.TotalMass:0.0} t • Długość: {_screen.Composition.TotalLengthMeters:0.0} m • Wagony: {_screen.Composition.WagonCount}";
        _createButton.Enabled = _screen.Composition.CanMove;
        _status.Text = _screen.StatusMessage;
    }
}
