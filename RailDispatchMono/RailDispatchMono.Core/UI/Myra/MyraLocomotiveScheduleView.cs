using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>Interactive editor for the locomotive-owned cyclic timetable.</summary>
internal sealed class MyraLocomotiveScheduleView
{
    public Widget Root { get; }
    private readonly Train _train;
    private readonly StationController _stations;
    private readonly Action _close;
    private readonly Locomotive _locomotive;
    private LocomotiveSchedule _draft;
    private readonly Label _status;
    private readonly VerticalStackPanel _pointsPanel;

    public MyraLocomotiveScheduleView(Train train, StationController stations, Action close)
    {
        _train = train;
        _stations = stations;
        _close = close;
        _locomotive = train.Composition.Locomotive ?? throw new InvalidOperationException("Pociąg nie ma lokomotywy.");
        _draft = _locomotive.Schedule?.Clone() ?? new LocomotiveSchedule();

        var root = new VerticalStackPanel { Width = 1100, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Spacing = 6 };
        root.Widgets.Add(new Label { Text = $"TIMETABLE LOKOMOTYWY — {train.Id.ToString()[..8]}", Wrap = true });
        root.Widgets.Add(new Label { Text = "Arrival = planowany przyjazd. Departure = wymagany odjazd. Spóźnienie jest przenoszone na kolejne punkty.", Wrap = true });

        var controls = new HorizontalStackPanel { Spacing = 5 };
        controls.Widgets.Add(CreateButton("DODAJ PUNKT", 150, AddPoint));
        controls.Widgets.Add(CreateButton("WŁĄCZ / WYŁĄCZ", 170, () => { _draft.Enabled = !_draft.Enabled; Refresh(); }));
        controls.Widgets.Add(CreateButton("ZAPISZ", 120, Save));
        controls.Widgets.Add(CreateButton("ANULUJ", 120, _close));
        root.Widgets.Add(controls);

        _pointsPanel = new VerticalStackPanel { Spacing = 4 };
        root.Widgets.Add(new ScrollViewer { Height = 620, Content = _pointsPanel });
        _status = new Label { Wrap = true };
        root.Widgets.Add(_status);
        Root = root;
        Refresh();
    }

    private void AddPoint()
    {
        var stations = _stations.Stations.ToList();
        if (stations.Count == 0) { _status.Text = "Brak stacji — najpierw zbuduj stacje."; return; }
        Guid stationId = stations[Math.Min(_draft.Points.Count, stations.Count - 1)].Id;
        int arrival = _draft.Points.Count == 0 ? 0 : _draft.Points[^1].DepartureSeconds + 300;
        int departure = arrival + 60;
        _draft.Points.Add(new LocomotiveSchedulePoint(stationId, Math.Min(86399, arrival), Math.Min(86399, departure)));
        Refresh();
    }

    private void Save()
    {
        if (!_draft.IsValid(out string error)) { _status.Text = "BŁĄD: " + error; return; }
        _locomotive.Schedule = _draft.Clone();
        _train.LocomotiveScheduleRuntime?.Reset(_locomotive.Schedule.Id);
        _close();
    }

    private void Refresh()
    {
        _pointsPanel.Widgets.Clear();
        var stations = _stations.Stations.ToList();
        for (int i = 0; i < _draft.Points.Count; i++)
        {
            int index = i;
            var point = _draft.Points[i];
            var station = stations.FirstOrDefault(s => s.Id == point.StationId);
            var row = new VerticalStackPanel { Spacing = 2 };
            row.Widgets.Add(new Label { Text = $"{i + 1}. {station?.Name ?? "BRAK STACJI"}   PRZYJAZD {FormatClock(point.ArrivalSeconds)}   ODJAZD {FormatClock(point.DepartureSeconds)}", Wrap = true });
            var buttons = new HorizontalStackPanel { Spacing = 3 };
            buttons.Widgets.Add(CreateButton("← STACJA", 90, () => ChangeStation(index, -1), stations.Count == 0));
            buttons.Widgets.Add(CreateButton("STACJA →", 90, () => ChangeStation(index, 1), stations.Count == 0));
            buttons.Widgets.Add(CreateButton("PRZYJ. −5m", 100, () => ChangeTime(index, -300, true)));
            buttons.Widgets.Add(CreateButton("PRZYJ. +5m", 100, () => ChangeTime(index, 300, true)));
            buttons.Widgets.Add(CreateButton("ODJ. −5m", 90, () => ChangeTime(index, -300, false)));
            buttons.Widgets.Add(CreateButton("ODJ. +5m", 90, () => ChangeTime(index, 300, false)));
            buttons.Widgets.Add(CreateButton("▲", 45, () => Move(index, -1), index == 0));
            buttons.Widgets.Add(CreateButton("▼", 45, () => Move(index, 1), index == _draft.Points.Count - 1));
            buttons.Widgets.Add(CreateButton("USUŃ", 70, () => { _draft.Points.RemoveAt(index); Refresh(); }));
            row.Widgets.Add(buttons);
            _pointsPanel.Widgets.Add(row);
        }
        if (_draft.Points.Count == 0) _pointsPanel.Widgets.Add(new Label { Text = "Brak punktów. Dodaj minimum dwa punkty." });
        string validity = _draft.IsValid(out string error) ? "POPRAWNY" : error;
        _status.Text = $"Status: {(_draft.Enabled ? "AUTO" : "WYŁĄCZONY")} • punkty: {_draft.Points.Count} • cykl: {FormatClock(_draft.CycleDurationSeconds)} • {validity}";
    }

    private void ChangeStation(int index, int direction)
    {
        var stations = _stations.Stations.ToList();
        if (stations.Count == 0 || index < 0 || index >= _draft.Points.Count) return;
        int current = stations.FindIndex(s => s.Id == _draft.Points[index].StationId);
        current = Math.Max(0, current);
        current = (current + direction + stations.Count) % stations.Count;
        _draft.Points[index].StationId = stations[current].Id;
        Refresh();
    }

    private void ChangeTime(int index, int delta, bool arrival)
    {
        if (index < 0 || index >= _draft.Points.Count) return;
        var point = _draft.Points[index];
        if (arrival) point.ArrivalSeconds = Math.Clamp(point.ArrivalSeconds + delta, 0, 86399);
        else point.DepartureSeconds = Math.Clamp(point.DepartureSeconds + delta, 0, 86399);
        Refresh();
    }

    private void Move(int index, int delta)
    {
        int target = index + delta;
        if (target < 0 || target >= _draft.Points.Count) return;
        (_draft.Points[index], _draft.Points[target]) = (_draft.Points[target], _draft.Points[index]);
        Refresh();
    }

    private static Button CreateButton(string text, int width, Action action, bool disabled = false)
    {
        var button = new Button { Content = new Label { Text = text, Wrap = true }, Width = width, HorizontalAlignment = HorizontalAlignment.Left, Enabled = !disabled };
        button.Click += (_, _) => action();
        return button;
    }

    private static string FormatClock(int seconds)
    {
        int normalized = ((seconds % 86400) + 86400) % 86400;
        return $"{normalized / 3600:D2}:{normalized / 60 % 60:D2}:{normalized % 60:D2}";
    }
}
