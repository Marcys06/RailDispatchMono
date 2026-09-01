using System;
using System.Collections.Generic;
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
    private readonly Grid _toolList;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;

    public MyraGameplayView(
        Action<float> setSpeed,
        Action<Train> focusTrain,
        Action<Station> focusStation,
        Action<TrackBuildMode> setBuildMode)
    {
        _setSpeed = setSpeed;
        _focusTrain = focusTrain;
        _focusStation = focusStation;
        _setBuildMode = setBuildMode;

        _grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ColumnSpacing = 8,
            RowSpacing = 6
        };

        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        for (int i = 0; i < 3; i++)
            _grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var clockPanel = new Grid { Width = 230, RowSpacing = 3 };
        clockPanel.RowsProportions.Add(new Proportion(ProportionType.Auto));
        clockPanel.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _clockLabel = new Label { Text = "00:00", HorizontalAlignment = HorizontalAlignment.Left };
        _dayLabel = new Label { Text = "Dzień 1", HorizontalAlignment = HorizontalAlignment.Left };
        Grid.SetRow(_clockLabel, 0);
        Grid.SetRow(_dayLabel, 1);
        clockPanel.Widgets.Add(_clockLabel);
        clockPanel.Widgets.Add(_dayLabel);
        Grid.SetColumn(clockPanel, 0);
        Grid.SetRow(clockPanel, 0);
        _grid.Widgets.Add(clockPanel);

        var speedPanel = new Grid { Width = 220, ColumnSpacing = 4 };
        speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        speedPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        AddSpeedButton(speedPanel, 0, "x1", 1f);
        AddSpeedButton(speedPanel, 1, "x2", 2f);
        AddSpeedButton(speedPanel, 2, "x5", 5f);
        Grid.SetColumn(speedPanel, 1);
        Grid.SetRow(speedPanel, 0);
        _grid.Widgets.Add(speedPanel);

        _speedLabel = new Label { Text = "Prędkość: x1", HorizontalAlignment = HorizontalAlignment.Right };
        Grid.SetColumn(_speedLabel, 2);
        Grid.SetRow(_speedLabel, 0);
        _grid.Widgets.Add(_speedLabel);

        _trainList = CreateListGrid(285);
        var trainTitle = new Label { Text = "POCIĄGI" };
        Grid.SetRow(trainTitle, 0);
        Grid.SetColumn(trainTitle, 0);
        _grid.Widgets.Add(trainTitle);
        Grid.SetRow(_trainList, 1);
        Grid.SetColumn(_trainList, 0);
        _grid.Widgets.Add(_trainList);

        _toolList = CreateListGrid(300);
        var toolTitle = new Label { Text = "NARZĘDZIA" };
        Grid.SetRow(toolTitle, 0);
        Grid.SetColumn(toolTitle, 1);
        _grid.Widgets.Add(toolTitle);
        Grid.SetRow(_toolList, 1);
        Grid.SetColumn(_toolList, 1);
        _grid.Widgets.Add(_toolList);

        _stationList = CreateListGrid(285);
        var stationTitle = new Label { Text = "STACJE" };
        Grid.SetRow(stationTitle, 0);
        Grid.SetColumn(stationTitle, 2);
        _grid.Widgets.Add(stationTitle);
        Grid.SetRow(_stationList, 1);
        Grid.SetColumn(_stationList, 2);
        _grid.Widgets.Add(_stationList);

        Root = _grid;
        Refresh();
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
        if (TrainManager.Current != null)
        {
            int row = 0;
            foreach (Train train in TrainManager.Current.Trains.Take(8))
            {
                var button = new Button
                {
                    Content = new Label { Text = $"Pociąg {train.Id.ToString()[..6]}  {train.Speed:0.0} m/s" },
                    Width = 275,
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
        if (TrainManager.Current != null)
        {
            int row = 0;
            foreach (Station station in TrainManager.Current.StationController.Stations.Take(8))
            {
                int passengers = TrainManager.Current.StationController.Passengers.GetWaitingCount(station);
                var button = new Button
                {
                    Content = new Label { Text = $"{station.Name}  [{passengers}]" },
                    Width = 275,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Station selected = station;
                button.Click += (_, _) => _focusStation(selected);
                Grid.SetRow(button, row++);
                _stationList.RowsProportions.Add(new Proportion(ProportionType.Auto));
                _stationList.Widgets.Add(button);
            }
        }

        _toolList.Widgets.Clear();
        AddToolButton("Tor prosty", TrackBuildMode.Straight);
        AddToolButton("Zakręt", TrackBuildMode.Curve);
        AddToolButton("Rozjazd", TrackBuildMode.Junction);
        AddToolButton("Semafor", TrackBuildMode.Signal);
        AddToolButton("Stacja", TrackBuildMode.Station);
        AddToolButton("Depot", TrackBuildMode.Depot);
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
        int row = _toolList.Widgets.Count;
        var button = new Button
        {
            Content = new Label { Text = text },
            Width = 290,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.Click += (_, _) => _setBuildMode(mode);
        Grid.SetRow(button, row);
        _toolList.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _toolList.Widgets.Add(button);
    }

    private static Grid CreateListGrid(int width)
        => new()
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left
        };
}
