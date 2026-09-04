using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

internal sealed class MyraWagonScheduleView
{
    public Widget Root { get; }

    private readonly Wagon _wagon;
    private readonly StationController _stations;
    private readonly Action _close;
    private readonly Action _saved;
    private readonly VerticalStackPanel _routePanel;
    private readonly VerticalStackPanel _timePanel;
    private readonly Label _status;
    private readonly List<Guid> _baseRoute = new();
    private readonly List<(TextBox Arrival, TextBox Departure)> _timeBoxes = new();
    private WagonSchedule _schedule;

    public MyraWagonScheduleView(Wagon wagon, StationController stations, Action saved, Action close)
    {
        _wagon = wagon;
        _stations = stations;
        _saved = saved;
        _close = close;
        _schedule = wagon.Schedule?.Clone() ?? new WagonSchedule();

        _baseRoute.AddRange(_schedule.BaseStationIds.Count >= 2
            ? _schedule.BaseStationIds
            : wagon.Route.StationIds);

        var root = new VerticalStackPanel
        {
            Width = 1250,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8
        };
        root.Widgets.Add(new Label
        {
            Text = $"ROZKŁAD WAGONU — {_wagon.ShortName}",
            HorizontalAlignment = HorizontalAlignment.Center
        });
        root.Widgets.Add(new Label
        {
            Text = "Trasa bazowa A-B-C-D jest rozwijana automatycznie do A-B-C-D-C-B-A. Wpisz osobno przyjazd i odjazd dla każdego punktu."
        });

        var columns = new Grid { Width = 1200, Height = 600, ColumnSpacing = 12 };
        columns.ColumnsProportions.Add(new Proportion(ProportionType.Part, 0.8f));
        columns.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.8f));

        var route = new VerticalStackPanel { Width = 360, Spacing = 4 };
        route.Widgets.Add(new Label { Text = "TRASA BAZOWA" });
        _routePanel = new VerticalStackPanel { Width = 360, Spacing = 3 };
        route.Widgets.Add(_routePanel);
        route.Widgets.Add(new Label { Text = "DODAJ STACJĘ:" });
        foreach (var station in _stations.Stations)
        {
            var stationButton = new Button
            {
                Content = new Label { Text = $"+ {station.Name}" },
                Width = 350
            };
            stationButton.Click += (_, _) => AddStation(station.Id);
            route.Widgets.Add(stationButton);
        }
        var clearRoute = new Button { Content = new Label { Text = "WYCZYŚĆ TRASĘ" }, Width = 350 };
        clearRoute.Click += (_, _) => { _baseRoute.Clear(); Rebuild(); };
        route.Widgets.Add(clearRoute);
        Grid.SetColumn(route, 0);
        columns.Widgets.Add(route);

        var timetable = new VerticalStackPanel { Width = 820, Spacing = 4 };
        timetable.Widgets.Add(new Label { Text = "PUNKTY KONTROLNE" });
        var header = new Grid { Width = 800, ColumnSpacing = 8 };
        header.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.5f));
        header.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        header.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        header.Widgets.Add(new Label { Text = "STACJA" });
        Grid.SetColumn(header.Widgets[^1], 0);
        header.Widgets.Add(new Label { Text = "PRZYJAZD HH:MM" });
        Grid.SetColumn(header.Widgets[^1], 1);
        header.Widgets.Add(new Label { Text = "ODJAZD HH:MM" });
        Grid.SetColumn(header.Widgets[^1], 2);
        timetable.Widgets.Add(header);
        _timePanel = new VerticalStackPanel { Width = 800, Spacing = 3 };
        timetable.Widgets.Add(_timePanel);
        Grid.SetColumn(timetable, 1);
        columns.Widgets.Add(timetable);
        root.Widgets.Add(columns);

        _status = new Label { Text = string.Empty };
        root.Widgets.Add(_status);
        var actions = new HorizontalStackPanel { HorizontalAlignment = HorizontalAlignment.Center, Spacing = 8 };
        var save = new Button { Content = new Label { Text = "ZAPISZ ROZKŁAD" }, Width = 190 };
        save.Click += (_, _) => Save();
        actions.Widgets.Add(save);
        var clear = new Button { Content = new Label { Text = "USUŃ ROZKŁAD" }, Width = 160 };
        clear.Click += (_, _) => { _wagon.SetSchedule(null); _saved(); _close(); };
        actions.Widgets.Add(clear);
        var cancel = new Button { Content = new Label { Text = "ANULUJ" }, Width = 130 };
        cancel.Click += (_, _) => _close();
        actions.Widgets.Add(cancel);
        root.Widgets.Add(actions);

        Root = root;
        Rebuild();
    }

    private void AddStation(Guid stationId)
    {
        if (_baseRoute.Count > 0 && _baseRoute[^1] == stationId)
        {
            _status.Text = "Nie można dodać tej samej stacji dwa razy z rzędu.";
            return;
        }
        _baseRoute.Add(stationId);
        Rebuild();
    }

    private void RemoveStation(int index)
    {
        if (index < 0 || index >= _baseRoute.Count) return;
        _baseRoute.RemoveAt(index);
        Rebuild();
    }

    private void Rebuild()
    {
        _routePanel.Widgets.Clear();
        for (int i = 0; i < _baseRoute.Count; i++)
        {
            int index = i;
            var station = _stations.Stations.FirstOrDefault(s => s.Id == _baseRoute[index]);
            var row = new HorizontalStackPanel { Spacing = 4 };
            row.Widgets.Add(new Label { Text = $"{(char)('A' + i)}. {station?.Name ?? "BRAK"}", Width = 290 });
            var remove = new Button { Content = new Label { Text = "USUŃ" }, Width = 55 };
            remove.Click += (_, _) => RemoveStation(index);
            row.Widgets.Add(remove);
            _routePanel.Widgets.Add(row);
        }

        _timePanel.Widgets.Clear();
        _timeBoxes.Clear();
        if (_baseRoute.Count < 2)
        {
            _status.Text = "Dodaj co najmniej dwie stacje.";
            return;
        }

        var loop = _baseRoute.Concat(_baseRoute.Skip(1).Reverse()).ToList();
        List<WagonSchedulePoint> previous = _schedule.Points.Count == loop.Count
            ? _schedule.Points
            : new List<WagonSchedulePoint>();

        for (int i = 0; i < loop.Count; i++)
        {
            var station = _stations.Stations.FirstOrDefault(s => s.Id == loop[i]);
            var old = previous.Count > i ? previous[i] : null;
            var row = new HorizontalStackPanel { Spacing = 8 };
            row.Widgets.Add(new Label
            {
                Text = $"{i + 1}. {station?.Name ?? "BRAK"}{(i == 0 || i == loop.Count - 1 ? "  [TERMINAL]" : "")}",
                Width = 260
            });
            var arrival = new TextBox { Text = FormatTime(old?.ArrivalSeconds ?? 0), Width = 180 };
            var departure = new TextBox { Text = FormatTime(old?.DepartureSeconds ?? 0), Width = 180 };
            row.Widgets.Add(arrival);
            row.Widgets.Add(departure);
            _timeBoxes.Add((arrival, departure));
            _timePanel.Widgets.Add(row);
        }

        _status.Text = $"Pętla: {string.Join(" → ", loop.Select(id => _stations.Stations.FirstOrDefault(s => s.Id == id)?.Name ?? "?"))}";
    }

    private void Save()
    {
        if (_baseRoute.Count < 2)
        {
            _status.Text = "Dodaj co najmniej dwie stacje.";
            return;
        }

        var schedule = new WagonSchedule
        {
            Id = _schedule.Id == Guid.Empty ? Guid.NewGuid() : _schedule.Id,
            Name = _wagon.ShortName + " — rozkład",
            BaseStationIds = _baseRoute.ToList(),
            Enabled = true
        };
        schedule.BuildLoopFromBaseRoute();

        if (_timeBoxes.Count != schedule.Points.Count)
        {
            _status.Text = "Nie udało się utworzyć wszystkich punktów rozkładu.";
            return;
        }

        for (int i = 0; i < _timeBoxes.Count; i++)
        {
            if (!TryParseTime(_timeBoxes[i].Arrival.Text, out int arrival) ||
                !TryParseTime(_timeBoxes[i].Departure.Text, out int departure))
            {
                _status.Text = $"Nieprawidłowy czas w punkcie {i + 1}. Użyj HH:MM.";
                return;
            }
            schedule.Points[i].ArrivalSeconds = arrival;
            schedule.Points[i].DepartureSeconds = departure;
        }

        if (!schedule.IsValid(out string error))
        {
            _status.Text = error;
            return;
        }

        _wagon.Route.Clear();
        foreach (Guid stationId in _baseRoute)
            _wagon.Route.AddStation(stationId);
        _wagon.SetSchedule(schedule);
        _saved();
        _close();
    }

    private static string FormatTime(int seconds)
    {
        if (seconds <= 0) return "00:00";
        return TimeSpan.FromSeconds(seconds).ToString("hh\\:mm", CultureInfo.InvariantCulture);
    }

    private static bool TryParseTime(string text, out int seconds)
    {
        seconds = 0;
        if (!TimeSpan.TryParseExact(text.Trim(), new[] { "hh\\:mm", "h\\:mm", "hh\\:mm\\:ss", "h\\:mm\\:ss" }, CultureInfo.InvariantCulture, out TimeSpan value))
            return false;
        if (value < TimeSpan.Zero || value.TotalSeconds >= 24 * 60 * 60) return false;
        seconds = (int)value.TotalSeconds;
        return true;
    }
}
