using System;
using System.Linq;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>
/// Myra presentation for the live gameplay HUD. It owns presentation only;
/// simulation, camera and building state remain owned by the existing gameplay systems.
/// </summary>
internal sealed class MyraGameplayView
{
    public Widget Root { get; }

    private readonly Grid _grid;
    private readonly Label _clockLabel;
    private readonly Label _dayLabel;
    private readonly Label _speedLabel;
    private readonly Grid _trainList;
    private readonly Grid _stationList;
    private readonly VerticalStackPanel _toolContent;
    private readonly Button _toolToggle;
    private readonly Grid _speedPanel;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;
    private readonly Action _toggleRouteEdit;
    private bool _toolsExpanded;

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

        _grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ColumnSpacing = 10,
            RowSpacing = 8
        };
        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _grid.RowsProportions.Add(new Proportion(ProportionType.Fill));
        _grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var clockPanel = new VerticalStackPanel { Width = 210, Spacing = 2 };
        _clockLabel = new Label { Text = "00:00", HorizontalAlignment = HorizontalAlignment.Left };
        _dayLabel = new Label { Text = "Dzień 1", HorizontalAlignment = HorizontalAlignment.Left };
        clockPanel.Widgets.Add(_clockLabel);
        clockPanel.Widgets.Add(_dayLabel);
        Grid.SetColumn(clockPanel, 0);
        Grid.SetRow(clockPanel, 0);
        _grid.Widgets.Add(clockPanel);

        _speedLabel = new Label
        {
            Text = "Prędkość: x1",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(_speedLabel, 1);
        Grid.SetRow(_speedLabel, 0);
        _grid.Widgets.Add(_speedLabel);

        _speedPanel = new Grid
        {
            Width = 180,
            Height = 34,
            ColumnSpacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        _speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        AddSpeedButton(_speedPanel, 0, "x1", 1f);
        AddSpeedButton(_speedPanel, 1, "x2", 2f);
        AddSpeedButton(_speedPanel, 2, "x5", 5f);
        Grid.SetColumn(_speedPanel, 2);
        Grid.SetRow(_speedPanel, 2);
        _grid.Widgets.Add(_speedPanel);

        _trainList = CreateListGrid(300);
        AddSection(_grid, "POCIĄGI", _trainList, 0);

        var toolsSection = new VerticalStackPanel
        {
            Width = 280,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _toolToggle = new Button
        {
            Content = new Label { Text = "NARZĘDZIA  ▼" },
            Width = 280,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _toolToggle.Click += (_, _) => ToggleTools();
        toolsSection.Widgets.Add(_toolToggle);

        _toolContent = new VerticalStackPanel
        {
            Width = 280,
            Spacing = 3,
            Visible = false,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        toolsSection.Widgets.Add(_toolContent);
        Grid.SetColumn(toolsSection, 1);
        Grid.SetRow(toolsSection, 1);
        _grid.Widgets.Add(toolsSection);

        _stationList = CreateListGrid(300);
        AddSection(_grid, "STACJE", _stationList, 2);

        Root = _grid;
        Refresh();
    }

    private static void AddSection(Grid parent, string title, Grid content, int column)
    {
        var section = new VerticalStackPanel
        {
            Spacing = 3,
            Width = content.Width,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        section.Widgets.Add(new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Left });
        section.Widgets.Add(content);
        Grid.SetColumn(section, column);
        Grid.SetRow(section, 1);
        parent.Widgets.Add(section);
    }

    public void Refresh()
    {
        GameClock? clock = GameClock.Current;
        if (clock != null)
        {
            _clockLabel.Text = clock.DisplayTime;
            _dayLabel.Text = $"Dzień {clock.GameDay}";
            _speedLabel.Text = $"Prędkość: x{clock.SimulationSpeed:0}";
        }

        _trainList.Widgets.Clear();
        _trainList.RowsProportions.Clear();
        if (TrainManager.Current != null)
        {
            int row = 0;
            foreach (Train train in TrainManager.Current.Trains.Take(8))
            {
                var button = new Button
                {
                    Content = new Label { Text = $"Pociąg {train.Id.ToString()[..6]}  {train.Speed:0.0} m/s" },
                    Width = 290,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Train selected = train;
                button.Click += (_, _) => _focusTrain(selected);
                Grid.SetRow(button, row++);
                _trainList.RowsProportions.Add(new Proportion(ProportionType.Auto));
                _trainList.Widgets.Add(button);
            }
        }

        _stationList.Widgets.Clear();
        _stationList.RowsProportions.Clear();
        if (TrainManager.Current != null)
        {
            int row = 0;
            foreach (Station station in TrainManager.Current.StationController.Stations.Take(8))
            {
                int passengers = TrainManager.Current.StationController.Passengers.GetWaitingCount(station);
                var button = new Button
                {
                    Content = new Label { Text = $"{station.Name}  [{passengers}]" },
                    Width = 290,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Station selected = station;
                button.Click += (_, _) => _focusStation(selected);
                Grid.SetRow(button, row++);
                _stationList.RowsProportions.Add(new Proportion(ProportionType.Auto));
                _stationList.Widgets.Add(button);
            }
        }

        RefreshTools();
    }

    private void RefreshTools()
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
        RefreshTools();
    }

    private void AddSpeedButton(Grid panel, int column, string text, float speed)
    {
        var button = new Button { Content = new Label { Text = text } };
        button.Click += (_, _) => _setSpeed(speed);
        Grid.SetColumn(button, column);
        panel.Widgets.Add(button);
    }

    private void AddToolButton(string text, TrackBuildMode mode)
    {
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 275,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.Click += (_, _) => _setBuildMode(mode);
        _toolContent.Widgets.Add(button);
    }

    private void AddRouteButton()
    {
        var button = new Button
        {
            Content = new Label { Text = "Edytuj trasę wagonu (S)" },
            Width = 275,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.Click += (_, _) => _toggleRouteEdit();
        _toolContent.Widgets.Add(button);
    }

    private static Grid CreateListGrid(int width)
        => new() { Width = width, HorizontalAlignment = HorizontalAlignment.Left };
}
