using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>
/// 0.2.1 gameplay dashboard. Myra owns presentation only; simulation/domain state stays in the game model.
/// The layout is deliberately compact, responsive and grouped by the dispatcher workflow.
/// </summary>
internal sealed class MyraGameplayView
{
    public Widget Root { get; }
    private readonly Label _clockLabel;
    private readonly Label _dayLabel;
    private readonly Label _simulationLabel;
    private readonly Label _modeLabel;
    private readonly Label _selectionLabel;
    private readonly Label _selectionDetails;
    private readonly Label _dispatcherLabel;
    private readonly Label _stationSummary;
    private readonly Label _wagonSummary;
    private readonly Label _toolHint;
    private readonly Grid _trainList;
    private readonly Grid _stationList;
    private readonly Grid _wagonList;
    private readonly VerticalStackPanel _toolContent;
    private readonly Button _toolToggle;
    private readonly Button _debugToggle;
    private readonly VerticalStackPanel _debugContent;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;
    private readonly Action _toggleRouteEdit;
    private readonly HashSet<Guid> _expandedStations = new();
    private Train? _selectedTrain;
    private bool _toolsExpanded;
    private bool _debugExpanded;

    public MyraGameplayView(Action<float> setSpeed, Action<Train> focusTrain, Action<Station> focusStation, Action<TrackBuildMode> setBuildMode, Action toggleRouteEdit)
    {
        _setSpeed = setSpeed;
        _focusTrain = train => { _selectedTrain = train; focusTrain(train); RefreshSelection(); };
        _focusStation = focusStation;
        _setBuildMode = setBuildMode;
        _toggleRouteEdit = toggleRouteEdit;

        var root = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, RowSpacing = 6, ColumnSpacing = 8 };
        root.RowsProportions.Add(new Proportion(ProportionType.Auto));
        root.RowsProportions.Add(new Proportion(ProportionType.Part, 1));
        root.RowsProportions.Add(new Proportion(ProportionType.Auto));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 280));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 350));

        var top = BuildTopBar(); Grid.SetRow(top, 0); Grid.SetColumnSpan(top, 3); root.Widgets.Add(top);
        var operations = BuildOperationsPanel(); Grid.SetRow(operations, 1); Grid.SetColumn(operations, 0); root.Widgets.Add(operations);
        var selection = BuildSelectionPanel(); Grid.SetRow(selection, 1); Grid.SetColumn(selection, 1); root.Widgets.Add(selection);
        var traffic = BuildTrafficPanel(); Grid.SetRow(traffic, 1); Grid.SetColumn(traffic, 2); root.Widgets.Add(traffic);
        var bottom = BuildBottomBar(); Grid.SetRow(bottom, 2); Grid.SetColumnSpan(bottom, 3); root.Widgets.Add(bottom);

        Root = root;
        Refresh();
    }

    public void Refresh()
    {
        var clock = GameClock.Current;
        if (clock != null)
        {
            _clockLabel.Text = clock.DisplayTime;
            _dayLabel.Text = $"DZIEŃ {clock.GameDay}";
            _simulationLabel.Text = $"SYMULACJA  x{clock.SimulationSpeed:0}";
        }

        var manager = TrainManager.Current;
        if (manager == null) { _modeLabel.Text = "BRAK SESJI"; return; }
        _modeLabel.Text = BuildModeText(manager);
        RebuildLists(manager);
        RefreshSelection();
        RefreshDebug(manager);
    }

    private Widget BuildTopBar()
    {
        var bar = new HorizontalStackPanel { Spacing = 14, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
        _clockLabel = new Label { Text = "00:00", Width = 105 };
        _dayLabel = new Label { Text = "DZIEŃ 1", Width = 80 };
        _simulationLabel = new Label { Text = "SYMULACJA x1", Width = 135 };
        _modeLabel = new Label { Text = "TRYB: NORMALNY", Width = 220, Wrap = true };
        bar.Widgets.Add(_clockLabel); bar.Widgets.Add(_dayLabel); bar.Widgets.Add(_simulationLabel); bar.Widgets.Add(_modeLabel);
        foreach (var item in new[] { ("1x", 1f), ("2x", 2f), ("5x", 5f) })
        {
            var button = CreateButton(item.Item1, 50); button.Click += (_, _) => _setSpeed(item.Item2); bar.Widgets.Add(button);
        }
        return bar;
    }

    private Widget BuildOperationsPanel()
    {
        var panel = new VerticalStackPanel { Width = 270, Spacing = 5 };
        panel.Widgets.Add(Header("OPERACJE"));
        panel.Widgets.Add(new Label { Text = "Sterowanie ruchem, infrastrukturą i składem. HUD nie wykonuje decyzji za dyspozytora.", Wrap = true });
        _toolToggle = CreateButton("NARZĘDZIA  ▼", 260);
        _toolToggle.Click += (_, _) => { _toolsExpanded = !_toolsExpanded; RebuildTools(); };
        panel.Widgets.Add(_toolToggle);
        _toolContent = new VerticalStackPanel { Width = 260, Spacing = 3, Visible = false };
        panel.Widgets.Add(_toolContent);
        panel.Widgets.Add(Header("PRĘDKOŚĆ SYMULACJI"));
        var speeds = new HorizontalStackPanel { Spacing = 4 };
        foreach (var item in new[] { ("1x", 1f), ("2x", 2f), ("5x", 5f) })
        {
            var button = CreateButton(item.Item1, 78); button.Click += (_, _) => _setSpeed(item.Item2); speeds.Widgets.Add(button);
        }
        panel.Widgets.Add(speeds);
        _toolHint = new Label { Text = "F6 manewry • F7 zmiana kierunku • C sprzęganie • X rozprzęganie • S trasa wagonu", Wrap = true };
        panel.Widgets.Add(_toolHint);
        return panel;
    }

    private Widget BuildSelectionPanel()
    {
        var panel = new VerticalStackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
        panel.Widgets.Add(Header("PANEL OPERACYJNY"));
        _selectionLabel = new Label { Text = "Nie wybrano pociągu.", Wrap = true };
        _selectionDetails = new Label { Text = "Kliknij pociąg na liście po prawej albo wybierz go na mapie.", Wrap = true };
        panel.Widgets.Add(_selectionLabel); panel.Widgets.Add(_selectionDetails);
        _debugToggle = CreateButton("DIAGNOSTYKA  ▼", 180);
        _debugToggle.Click += (_, _) => { _debugExpanded = !_debugExpanded; _debugContent.Visible = _debugExpanded; _debugToggle.Content = new Label { Text = _debugExpanded ? "DIAGNOSTYKA  ▲" : "DIAGNOSTYKA  ▼" }; RefreshDebug(TrainManager.Current); };
        panel.Widgets.Add(_debugToggle);
        _debugContent = new VerticalStackPanel { Spacing = 3, Visible = false };
        panel.Widgets.Add(_debugContent);
        return panel;
    }

    private Widget BuildTrafficPanel()
    {
        var panel = new VerticalStackPanel { Width = 340, Spacing = 5 };
        panel.Widgets.Add(Header("RUCH"));
        _dispatcherLabel = new Label { Text = "Dyspozytor: —", Wrap = true }; panel.Widgets.Add(_dispatcherLabel);
        panel.Widgets.Add(Header("POCIĄGI"));
        _trainList = CreateListGrid(335); panel.Widgets.Add(new ScrollViewer { Height = 205, Content = _trainList });
        panel.Widgets.Add(Header("STACJE"));
        _stationSummary = new Label { Text = "—", Wrap = true }; panel.Widgets.Add(_stationSummary);
        _stationList = CreateListGrid(335); panel.Widgets.Add(new ScrollViewer { Height = 120, Content = _stationList });
        return panel;
    }

    private Widget BuildBottomBar()
    {
        var panel = new HorizontalStackPanel { Spacing = 10, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Bottom };
        var wagonPanel = new VerticalStackPanel { Width = 600, Spacing = 3 };
        wagonPanel.Widgets.Add(Header("WAGONY"));
        _wagonSummary = new Label { Text = "Brak wagonów.", Wrap = true }; wagonPanel.Widgets.Add(_wagonSummary);
        _wagonList = CreateListGrid(590); wagonPanel.Widgets.Add(new ScrollViewer { Height = 90, Content = _wagonList }); panel.Widgets.Add(wagonPanel);
        var help = new VerticalStackPanel { Width = 600, Spacing = 2 };
        help.Widgets.Add(Header("STEROWANIE"));
        help.Widgets.Add(new Label { Text = "AUTOMAT: rozkład + bloki + sygnały  |  RĘCZNIE: zwrotnice + sygnały + manewry  |  RADIOSTOP zatrzymuje automat", Wrap = true });
        panel.Widgets.Add(help);
        return panel;
    }

    private void RebuildLists(TrainManager manager)
    {
        _trainList.Widgets.Clear(); _trainList.RowsProportions.Clear();
        _stationList.Widgets.Clear(); _stationList.RowsProportions.Clear();
        _wagonList.Widgets.Clear(); _wagonList.RowsProportions.Clear();

        int automaticCount = 0;
        foreach (var train in manager.Trains.Take(9))
        {
            if (train.LocomotiveSchedule?.Enabled == true) automaticCount++;
            string marker = train.LocomotiveSchedule?.Enabled == true ? "AUTO" : "RĘCZ";
            string selected = _selectedTrain?.Id == train.Id ? "  ◀" : string.Empty;
            AddListButton(_trainList, $"{marker}  {train.Id.ToString()[..6]}  {train.Speed * 3.6f:0.0} km/h{selected}", () => _focusTrain(train));
        }

        _dispatcherLabel.Text = $"Pociągi: {manager.Trains.Count} • automatyczne: {automaticCount}\n" + (_selectedTrain == null ? "Wybierz skład, aby zobaczyć stan drogi." : BuildDispatcherStatus(_selectedTrain));

        int stationTotal = 0;
        foreach (var station in manager.StationController.Stations.Take(5))
        {
            int waiting = manager.StationController.Passengers.GetWaitingCount(station); stationTotal += waiting;
            AddStationButton(_stationList, station, waiting, manager);
        }
        _stationSummary.Text = $"Oczekujący pasażerowie: {stationTotal}";

        int wagonTotal = 0; int passengerTotal = 0;
        foreach (var train in manager.Trains)
        {
            foreach (var wagon in train.Composition.Vehicles.OfType<Wagon>())
            {
                wagonTotal++; passengerTotal += wagon.PassengerCount;
                AddListButton(_wagonList, $"{wagon.ShortName}  {wagon.PassengerCount}/{wagon.PassengerCapacity}  opóźn. {FormatDelay(wagon.ScheduleRuntime.DelaySeconds, wagon.Schedule != null)}", () => _focusTrain(train), BuildWagonTooltip(wagon, manager.StationController));
                if (_wagonList.Widgets.Count >= 4) break;
            }
        }
        _wagonSummary.Text = $"Wagony: {wagonTotal} • pasażerowie w składach: {passengerTotal}";
        RebuildTools();
    }

    private void AddStationButton(Grid grid, Station station, int waiting, TrainManager manager)
    {
        bool expanded = _expandedStations.Contains(station.Id);
        var button = new Button { Content = new Label { Text = expanded ? BuildStationBreakdown(station, manager) : $"{station.Name}  •  oczekują: {waiting}", Wrap = true }, HorizontalAlignment = HorizontalAlignment.Stretch };
        button.Click += (_, _) => { if (!_expandedStations.Add(station.Id)) _expandedStations.Remove(station.Id); RebuildLists(manager); };
        Grid.SetRow(button, grid.Widgets.Count); grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); grid.Widgets.Add(button);
    }

    private static string BuildStationBreakdown(Station station, TrainManager manager)
    {
        var groups = manager.StationController.Passengers.GetWaitingAt(station).GroupBy(p => p.DestinationStation.Id).Select(g => new { Name = g.First().DestinationStation.Name, Count = g.Count() }).OrderByDescending(x => x.Count).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).Take(5).ToList();
        if (groups.Count == 0) return $"{station.Name}  •  0 oczekujących";
        return $"{station.Name}\n" + string.Join("\n", groups.Select(x => $"→ {x.Name}: {x.Count}"));
    }

    private static string BuildWagonTooltip(Wagon wagon, StationController stations)
    {
        var ids = wagon.Schedule?.Points.Select(p => p.StationId).ToList() ?? wagon.ServiceRoute.ToList();
        if (ids.Count == 0) return "Brak trasy wagonu.";
        return "TRASA WAGONU\n" + string.Join("\n", ids.Select((id, i) => $"{i + 1}. {stations.Stations.FirstOrDefault(s => s.Id == id)?.Name ?? id.ToString()[..8]}"));
    }

    private void RefreshSelection()
    {
        if (_selectedTrain == null || TrainManager.Current == null || !TrainManager.Current.Trains.Contains(_selectedTrain))
        {
            _selectionLabel.Text = "Nie wybrano pociągu."; _selectionDetails.Text = "Wybierz skład na liście RUCH."; return;
        }
        var train = _selectedTrain;
        string mode = train.LocomotiveSchedule?.Enabled == true ? "AUTOMAT" : "RĘCZNE";
        _selectionLabel.Text = $"Pociąg {train.Id.ToString()[..8]}  •  {mode}  •  {train.Speed * 3.6f:0.0} km/h";
        _selectionDetails.Text = $"Vmax składu: {train.MaxSpeed * 3.6f:0.0} km/h\nCel prędkości: {train.EffectiveTargetSpeed * 3.6f:0.0} km/h\nKierunek: {train.Direction}  •  {BuildDispatcherStatus(train)}\n{BuildLocomotiveScheduleSummary(train)}";
    }

    private void RefreshDebug(TrainManager? manager)
    {
        if (!_debugExpanded) return;
        _debugContent.Widgets.Clear();
        if (_selectedTrain == null || manager == null) { _debugContent.Widgets.Add(new Label { Text = "Brak wybranego pociągu." }); return; }
        var train = _selectedTrain; var composition = train.Composition;
        _debugContent.Widgets.Add(new Label { Text = $"ID: {train.Id}", Wrap = true });
        _debugContent.Widgets.Add(new Label { Text = $"Skład: {composition.Vehicles.Count} pojazdów • {composition.TotalMass:0.0} t • {composition.TotalLengthMeters:0.0} m", Wrap = true });
        _debugContent.Widgets.Add(new Label { Text = $"Ruch: {train.Speed * 3.6f:0.00}/{train.EffectiveTargetSpeed * 3.6f:0.00} km/h • hamowanie {train.EffectiveBrakingRate:0.000} m/s²", Wrap = true });
        _debugContent.Widgets.Add(new Label { Text = $"Dispatcher: {BuildDispatcherStatus(train)}", Wrap = true });
        _debugContent.Widgets.Add(new Label { Text = $"RadioStop: {(train.IsRadioStopped ? "AKTYWNY" : "nieaktywny")}", Wrap = true });
    }

    private static string BuildDispatcherStatus(Train train) => RailwayDispatcher.Current?.GetStatus(train) ?? "DISPATCHER NIEDOSTĘPNY";

    private static string BuildLocomotiveScheduleSummary(Train train)
    {
        var schedule = train.LocomotiveSchedule; var runtime = train.LocomotiveScheduleRuntime;
        if (schedule == null || !schedule.Enabled) return "Rozkład lokomotywy: brak / sterowanie ręczne.";
        if (runtime == null || runtime.CurrentPointIndex < 0) return $"Rozkład lokomotywy: {schedule.Name} • oczekiwanie na pierwszy punkt.";
        var point = schedule.Points[Math.Clamp(runtime.CurrentPointIndex, 0, schedule.Points.Count - 1)];
        return $"Rozkład: {schedule.Name}\nPunkt: {runtime.CurrentPointIndex + 1}/{schedule.Points.Count} • opóźnienie: {FormatDelay(runtime.DelaySeconds, true)}\nPlanowany odjazd: {FormatClock(point.DepartureSeconds)}";
    }

    private static string BuildModeText(TrainManager manager) => $"TRYB: NORMALNY  •  POCIĄGI {manager.Trains.Count}  •  BLOKI {manager.BlockController?.BlockCount ?? 0}";

    private void RebuildTools()
    {
        _toolContent.Widgets.Clear();
        AddTool("Tor prosty", TrackBuildMode.Straight); AddTool("Zakręt", TrackBuildMode.Curve); AddTool("Rozjazd", TrackBuildMode.Junction); AddTool("Semafor", TrackBuildMode.Signal); AddTool("Stacja", TrackBuildMode.Station); AddTool("Depot", TrackBuildMode.Depot);
        var route = CreateButton("TRASA WAGONU  [S]", 260); route.Click += (_, _) => _toggleRouteEdit(); _toolContent.Widgets.Add(route);
        _toolContent.Visible = _toolsExpanded; _toolToggle.Content = new Label { Text = _toolsExpanded ? "NARZĘDZIA  ▲" : "NARZĘDZIA  ▼" };
    }

    private void AddTool(string text, TrackBuildMode mode) { var button = CreateButton(text, 260); button.Click += (_, _) => _setBuildMode(mode); _toolContent.Widgets.Add(button); }
    private static Label Header(string text) => new() { Text = text, Wrap = true };
    private static Grid CreateListGrid(int width) => new() { Width = width, HorizontalAlignment = HorizontalAlignment.Left, RowSpacing = 2 };
    private static void AddListButton(Grid grid, string text, Action action, string? tooltip = null) { var button = new Button { Content = new Label { Text = text, Wrap = true }, HorizontalAlignment = HorizontalAlignment.Stretch, Tooltip = tooltip }; button.Click += (_, _) => action(); Grid.SetRow(button, grid.Widgets.Count); grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); grid.Widgets.Add(button); }
    private static Button CreateButton(string text, int width) => new() { Content = new Label { Text = text, Wrap = true }, Width = width, HorizontalAlignment = HorizontalAlignment.Stretch };
    private static string FormatDelay(int seconds, bool hasSchedule) { if (!hasSchedule) return "—"; if (seconds == 0) return "0 s"; string sign = seconds > 0 ? "+" : "−"; int abs = Math.Abs(seconds); int minutes = abs / 60; int rest = abs % 60; return minutes > 0 ? $"{sign}{minutes}m {rest:D2}s" : $"{sign}{rest}s"; }
    private static string FormatClock(int seconds) { int normalized = ((seconds % 86400) + 86400) % 86400; return $"{normalized / 3600:D2}:{normalized / 60 % 60:D2}"; }
}
