using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>Full Myra gameplay HUD. World rendering remains outside Myra.</summary>
internal sealed class MyraGameplayView
{
    public Widget Root { get; }
    private readonly Grid _left;
    private readonly Grid _right;
    private readonly Grid _trainList;
    private readonly Grid _stationList;
    private readonly VerticalStackPanel _toolContent;
    private readonly Button _toolToggle;
    private readonly Label _clockLabel;
    private readonly Label _dayLabel;
    private readonly Label _speedLabel;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;
    private readonly Action _toggleRouteEdit;
    private bool _toolsExpanded;
    private readonly Grid _speedGrid;

    public MyraGameplayView(
        Action<float> setSpeed,
        Action<Train> focusTrain,
        Action<Station> focusStation,
        Action<TrackBuildMode> setBuildMode,
        Action toggleRouteEdit)
    {
        _setSpeed = setSpeed;
        _focusTrain = focusTrain;
        _focusStation = focusStation;
        _setBuildMode = setBuildMode;
        _toggleRouteEdit = toggleRouteEdit;

        var root = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ColumnSpacing = 12,
            RowSpacing = 6
        };
        root.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

        _left = new Grid
        {
            Width = 300,
            RowSpacing = 5,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        for (int i = 0; i < 4; i++)
            _left.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var clock = new VerticalStackPanel
        {
            Width = 280,
            Spacing = 1,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _clockLabel = new Label { Text = "00:00", HorizontalAlignment = HorizontalAlignment.Left };
        _dayLabel = new Label { Text = "Dzień 1", HorizontalAlignment = HorizontalAlignment.Left };
        clock.Widgets.Add(_clockLabel);
        clock.Widgets.Add(_dayLabel);
        Grid.SetRow(clock, 0);
        _left.Widgets.Add(clock);

        _speedGrid = new Grid
        {
            Width = 280,
            Height = 36,
            ColumnSpacing = 4,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _speedGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        _speedGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        _speedGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));

        AddSpeedButton(_speedGrid, 0, "x1", 1f);
        AddSpeedButton(_speedGrid, 1, "x2", 2f);
        AddSpeedButton(_speedGrid, 2, "x5", 5f);
        Grid.SetRow(_speedGrid, 1);
        _left.Widgets.Add(_speedGrid);

        _speedLabel = new Label
        {
            Text = "Prędkość symulacji: x1",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_speedLabel, 2);
        _left.Widgets.Add(_speedLabel);

        var tools = new VerticalStackPanel
        {
            Width = 280,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _toolToggle = new Button
        {
            Content = new Label { Text = "NARZĘDZIA  ▼" },
            Width = 280,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _toolToggle.Click += (_, _) => ToggleTools();
        tools.Widgets.Add(_toolToggle);

        _toolContent = new VerticalStackPanel
        {
            Width = 280,
            Spacing = 3,
            Visible = false,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        tools.Widgets.Add(_toolContent);
        Grid.SetRow(tools, 3);
        _left.Widgets.Add(tools);

        Grid.SetColumn(_left, 0);
        Grid.SetRow(_left, 0);
        root.Widgets.Add(_left);

        _right = new Grid
        {
            Width = 360,
            RowSpacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        _right.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _right.RowsProportions.Add(new Proportion(ProportionType.Auto));

        _trainList = CreateListGrid(350);
        _stationList = CreateListGrid(350);

        AddInfoSection(_right, "POCIĄGI", _trainList, 0);
        AddInfoSection(_right, "STACJE", _stationList, 1);

        Grid.SetColumn(_right, 1);
        Grid.SetRow(_right, 0);
        root.Widgets.Add(_right);

        Root = root;
        Refresh();
    }

    private static void AddInfoSection(Grid parent, string title, Grid content, int row)
    {
        var section = new VerticalStackPanel
        {
            Width = content.Width,
            Spacing = 3,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        section.Widgets.Add(new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Left });
        section.Widgets.Add(content);
        Grid.SetRow(section, row);
        parent.Widgets.Add(section);
    }

    public void Refresh()
    {
        var clock = GameClock.Current;
        if (clock != null)
        {
            _clockLabel.Text = clock.DisplayTime;
            _dayLabel.Text = $"Dzień {clock.GameDay}";
            _speedLabel.Text = $"Prędkość symulacji: x{clock.SimulationSpeed:0}";
        }
        RebuildLists();
        RebuildTools();
    }

    private void RebuildLists()
    {
        _trainList.Widgets.Clear();
        _trainList.RowsProportions.Clear();
        _stationList.Widgets.Clear();
        _stationList.RowsProportions.Clear();

        var manager = TrainManager.Current;
        if (manager == null) return;

        foreach (var train in manager.Trains.Take(10))
        {
            float speedKmh = train.Speed * 3.6f;
            AddListButton(_trainList,
                $"Pociąg {train.Id.ToString()[..6]}  {speedKmh:0.0} km/h",
                () => _focusTrain(train));
        }

        foreach (var station in manager.StationController.Stations.Take(10))
        {
            int passengers = manager.StationController.Passengers.GetWaitingCount(station);
            AddListButton(_stationList,
                $"{station.Name}  •  pasażerowie: {passengers}",
                () => _focusStation(station));
        }
    }

    private void RebuildTools()
    {
        _toolContent.Widgets.Clear();
        AddToolButton("Tor prosty", TrackBuildMode.Straight);
        AddToolButton("Zakręt", TrackBuildMode.Curve);
        AddToolButton("Rozjazd", TrackBuildMode.Junction);
        AddToolButton("Semafor", TrackBuildMode.Signal);
        AddToolButton("Stacja", TrackBuildMode.Station);
        AddToolButton("Depot", TrackBuildMode.Depot);
        AddRouteButton();

        _toolContent.Visible = _toolsExpanded;
        _toolToggle.Content = new Label { Text = _toolsExpanded ? "NARZĘDZIA  ▲" : "NARZĘDZIA  ▼" };
    }

    private void ToggleTools()
    {
        _toolsExpanded = !_toolsExpanded;
        RebuildTools();
    }

    private void AddSpeedButton(Grid panel, int column, string text, float speed)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(2)
        };
        button.Click += (_, _) => _setSpeed(speed);
        Grid.SetColumn(button, column);
        panel.Widgets.Add(button);
    }

    private static void AddListButton(Grid grid, string text, Action action)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        button.Click += (_, _) => action();
        Grid.SetRow(button, grid.Widgets.Count);
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.Widgets.Add(button);
    }

    private void AddToolButton(string text, TrackBuildMode mode)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 280,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        button.Click += (_, _) => _setBuildMode(mode);
        _toolContent.Widgets.Add(button);
    }

    private void AddRouteButton()
    {
        var button = new Button
        {
            Content = new Label { Text = "Edytuj trasę wagonu (S)" },
            Width = 280,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        button.Click += (_, _) => _toggleRouteEdit();
        _toolContent.Widgets.Add(button);
    }

    private static Grid CreateListGrid(int width)
    {
        return new Grid { Width = width, HorizontalAlignment = HorizontalAlignment.Left };
    }
}
