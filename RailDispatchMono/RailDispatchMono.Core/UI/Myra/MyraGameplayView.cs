using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>0.2.3 operational gameplay dashboard. Presentation stays outside the simulation model.</summary>
internal sealed class MyraGameplayView
{
    public Widget Root { get; }
    private Label _clockLabel;
    private Label _dayLabel;
    private Label _simulationLabel;
    private Label _modeLabel;
    private Label _notificationLabel;
    private Label _selectionLabel;
    private Label _selectionDetails;
    private Label _dispatcherLabel;
    private Label _stationSummary;
    private Label _wagonSummary;
    private Grid _trainList;
    private Grid _stationList;
    private Grid _wagonList;
    private VerticalStackPanel _toolContent;
    private Button _toolToggle;
    private Button _debugToggle;
    private VerticalStackPanel _debugContent;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;
    private readonly Action _toggleRouteEdit;
    private readonly Action<Train> _forceProceed;
    private readonly Action<Train> _releaseRoute;
    private readonly Action<Train> _toggleAutomation;
    private readonly HashSet<Guid> _expandedStations = new();
    private Train? _selectedTrain;
    private bool _toolsExpanded;
    private bool _debugExpanded;

    public MyraGameplayView(Action<float> setSpeed, Action<Train> focusTrain, Action<Station> focusStation, Action<TrackBuildMode> setBuildMode, Action toggleRouteEdit, Action<Train> forceProceed, Action<Train> releaseRoute, Action<Train> toggleAutomation)
    {
        _setSpeed = setSpeed;
        _focusTrain = train => { _selectedTrain = train; focusTrain(train); RefreshSelection(); };
        _focusStation = focusStation;
        _setBuildMode = setBuildMode;
        _toggleRouteEdit = toggleRouteEdit;
        _forceProceed = forceProceed;
        _releaseRoute = releaseRoute;
        _toggleAutomation = toggleAutomation;

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
            _simulationLabel.Text = $"SYMULACJA x{clock.SimulationSpeed:0}";
        }
        var manager = TrainManager.Current;
        if (manager == null) { _modeLabel.Text = "BRAK SESJI"; return; }
        _modeLabel.Text = BuildModeText(manager);
        RebuildLists(manager);
        RefreshSelection();
        RefreshDebug(manager);
        RefreshNotification(manager);
    }

    private Widget BuildTopBar()
    {
        var bar = new HorizontalStackPanel { Spacing = 10, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
        _clockLabel = new Label { Text = "00:00", Width = 90 };
        _dayLabel = new Label { Text = "DZIEŃ 1", Width = 80 };
        _simulationLabel = new Label { Text = "SYMULACJA x1", Width = 125 };
        _modeLabel = new Label { Text = "TRYB: NORMALNY", Width = 210, Wrap = true };
        _notificationLabel = new Label { Text = "", Width = 520, Wrap = true };
        bar.Widgets.Add(_clockLabel); bar.Widgets.Add(_dayLabel); bar.Widgets.Add(_simulationLabel); bar.Widgets.Add(_modeLabel); bar.Widgets.Add(_notificationLabel);
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
        panel.Widgets.Add(new Label { Text = "HUD pokazuje sytuację operacyjną; nie przestawia zwrotnic ani semaforów za gracza.", Wrap = true });
        _toolToggle = CreateButton("NARZĘDZIA ▼", 260);
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
        panel.Widgets.Add(new Label { Text = "F6 wymuszenie przejazdu • F7 kierunek • C sprzęganie • X rozprzęganie • S trasa wagonu • F9 timetable • F10 diagnostyka", Wrap = true });
        return panel;
    }

    private Widget BuildSelectionPanel()
    {
        var panel = new VerticalStackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
        panel.Widgets.Add(Header("PANEL WYBRANEGO POCIĄGU"));
        _selectionLabel = new Label { Text = "Nie wybrano pociągu.", Wrap = true };
        _selectionDetails = new Label { Text = "Wybierz skład z listy RUCH.", Wrap = true };
        panel.Widgets.Add(_selectionLabel); panel.Widgets.Add(_selectionDetails);
        var actions = new HorizontalStackPanel { Spacing = 4 };
        var force = CreateButton("WYMUSZENIE [F6]", 155); force.Click += (_, _) => { if (_selectedTrain != null) _forceProceed(_selectedTrain); };
        var release = CreateButton("ZWOLNIJ TRASĘ", 135); release.Click += (_, _) => { if (_selectedTrain != null) _releaseRoute(_selectedTrain); };
        var auto = CreateButton("AUTO / RĘCZ", 120); auto.Click += (_, _) => { if (_selectedTrain != null) _toggleAutomation(_selectedTrain); };
        actions.Widgets.Add(force); actions.Widgets.Add(release); actions.Widgets.Add(auto); panel.Widgets.Add(actions);
        _debugToggle = CreateButton("DIAGNOSTYKA ▼", 180);
        _debugToggle.Click += (_, _) => { _debugExpanded = !_debugExpanded; _debugContent.Visible = _debugExpanded; _debugToggle.Content = new Label { Text = _debugExpanded ? "DIAGNOSTYKA ▲" : "DIAGNOSTYKA ▼" }; RefreshDebug(TrainManager.Current); };
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
        _trainList = CreateListGrid(335); panel.Widgets.Add(new ScrollViewer { Height = 235, Content = _trainList });
        panel.Widgets.Add(Header("STACJE"));
        _stationSummary = new Label { Text = "—", Wrap = true }; panel.Widgets.Add(_stationSummary);
        _stationList = CreateListGrid(335); panel.Widgets.Add(new ScrollViewer { Height = 100, Content = _stationList });
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
        help.Widgets.Add(new Label { Text = "AUTO: timetable + dispatcher • RĘCZ: decyzje gracza • F6 omija arbitraż dispatchera, ale nie zmienia fizycznego stanu bloku • RadioStop pozostaje nadrzędny.", Wrap = true });
        panel.Widgets.Add(help);
        return panel;
    }

    private void RebuildLists(TrainManager manager)
    {
        _trainList.Widgets.Clear(); _trainList.RowsProportions.Clear();
        _stationList.Widgets.Clear(); _stationList.RowsProportions.Clear();
        _wagonList.Widgets.Clear(); _wagonList.RowsProportions.Clear();

        int automaticCount = 0;
        foreach (var train in manager.Trains.Take(12))
        {
            bool auto = train.LocomotiveSchedule?.Enabled == true;
            if (auto) automaticCount++;
            string marker = auto ? "AUTO" : "RĘCZ";
            string point = BuildPointMarker(train);
            string delay = BuildDelayMarker(train);
            string selected = _selectedTrain?.Id == train.Id ? " ◀" : string.Empty;
            AddListButton(_trainList, $"{marker}  {train.Id.ToString()[..6]}  {point}  {delay}  {train.Speed * 3.6f:0.0} km/h{selected}", () => _focusTrain(train), BuildTrainTooltip(train));
        }

        _dispatcherLabel.Text = $"Pociągi: {manager.Trains.Count} • AUTO: {automaticCount}\n" + (_selectedTrain == null ? "Wybierz skład." : BuildDispatcherStatus(_selectedTrain));

        int stationTotal = 0;
        foreach (var station in manager.StationController.Stations.Take(5))
        {
            int waiting = manager.StationController.Passengers.GetWaitingCount(station); stationTotal += waiting;
            AddStationButton(_stationList, station, waiting, manager);
        }
        _stationSummary.Text = $"Oczekujący pasażerowie: {stationTotal}";

        int wagonTotal = 0; int passengerTotal = 0;
        foreach (var train in manager.Trains)
        foreach (var wagon in train.Composition.Vehicles.OfType<Wagon>())
        {
            wagonTotal++; passengerTotal += wagon.PassengerCount;
            AddListButton(_wagonList, $"{wagon.ShortName}  {wagon.PassengerCount}/{wagon.PassengerCapacity}  opóźn. {FormatDelay(wagon.ScheduleRuntime.DelaySeconds, wagon.Schedule != null)}", () => _focusTrain(train), BuildWagonTooltip(wagon, manager.StationController));
            if (_wagonList.Widgets.Count >= 4) break;
        }
        _wagonSummary.Text = $"Wagony: {wagonTotal} • pasażerowie w składach: {passengerTotal}";
        RebuildTools();
    }

    private static string BuildPointMarker(Train train)
    {
        var schedule = train.LocomotiveSchedule; var runtime = train.LocomotiveScheduleRuntime;
        if (schedule == null || !schedule.Enabled || runtime == null || runtime.CurrentPointIndex < 0) return "—";
        string name = schedule.Points[Math.Clamp(runtime.CurrentPointIndex, 0, schedule.Points.Count - 1)].StationId.ToString()[..4];
        return $"{name} {runtime.CurrentPointIndex + 1}/{schedule.Points.Count}";
    }

    private static string BuildDelayMarker(Train train)
    {
        var runtime = train.LocomotiveScheduleRuntime;
        if (train.LocomotiveSchedule?.Enabled != true || runtime == null) return "—";
        int d = runtime.DelaySeconds;
        return d <= 0 ? "ON TIME" : d < 120 ? $"+{d}s" : $"+{d / 60}m {d % 60:D2}s";
    }

    private static string BuildTrainTooltip(Train train)
    {
        var runtime = train.LocomotiveScheduleRuntime;
        if (train.LocomotiveSchedule?.Enabled != true || runtime == null) return "RĘCZNY: brak aktywnego timetable lokomotywy.";
        return $"TIMETABLE: {runtime.CurrentPointIndex + 1}/{train.LocomotiveSchedule.Points.Count}\nOpóźnienie: {FormatDelay(runtime.DelaySeconds, true)}\nStan: {runtime.State}";
    }

    private void AddStationButton(Grid grid, Station station, int waiting, TrainManager manager)
    {
        bool expanded = _expandedStations.Contains(station.Id);
        var button = new Button { Content = new Label { Text = expanded ? BuildStationBreakdown(station, manager) : $"{station.Name} • oczekują: {waiting}", Wrap = true }, HorizontalAlignment = HorizontalAlignment.Stretch };
        button.Click += (_, _) => { if (!_expandedStations.Add(station.Id)) _expandedStations.Remove(station.Id); RebuildLists(manager); };
        Grid.SetRow(button, grid.Widgets.Count); grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); grid.Widgets.Add(button);
    }

    private static string BuildStationBreakdown(Station station, TrainManager manager)
    {
        var groups = manager.StationController.Passengers.GetWaitingAt(station).GroupBy(p => p.DestinationStation.Id).Select(g => new { Name = g.First().DestinationStation.Name, Count = g.Count() }).OrderByDescending(x => x.Count).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).Take(5).ToList();
        return groups.Count == 0 ? $"{station.Name} • 0 oczekujących" : $"{station.Name}\n" + string.Join("\n", groups.Select(x => $"→ {x.Name}: {x.Count}"));
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
        string mode = train.LocomotiveSchedule?.Enabled == true ? "AUTO" : "RĘCZ";
        _selectionLabel.Text = $"Pociąg {train.Id.ToString()[..8]} • {mode} • {train.Speed * 3.6f:0.0} km/h";
        _selectionDetails.Text = $"Vmax: {train.MaxSpeed * 3.6f:0.0} km/h\nCel: {train.EffectiveTargetSpeed * 3.6f:0.0} km/h\nKierunek: {train.Direction}\nDispatcher: {BuildDispatcherStatus(train)}\n{BuildLocomotiveScheduleSummary(train)}";
    }

    private void RefreshDebug(TrainManager? manager)
    {
        if (!_debugExpanded) return;
        _debugContent.Widgets.Clear();
        if (_selectedTrain == null || manager == null) { _debugContent.Widgets.Add(new Label { Text = "Brak wybranego pociągu." }); return; }
        var train = _selectedTrain; var composition = train.Composition;
        _debugContent.Widgets.Add(new Label { Text = $"ID: {train.Id}\nSkład: {composition.Vehicles.Count} pojazdów • {composition.TotalMass:0.0} t • {composition.TotalLengthMeters:0.0} m", Wrap = true });
        _debugContent.Widgets.Add(new Label { Text = $"Ruch: {train.Speed * 3.6f:0.00}/{train.EffectiveTargetSpeed * 3.6f:0.00} km/h • hamowanie {train.EffectiveBrakingRate:0.000} m/s²\nRadioStop: {(train.IsRadioStopped ? "AKTYWNY" : "nieaktywny")}", Wrap = true });
    }

    private void RefreshNotification(TrainManager manager)
    {
        if (_selectedTrain == null) { _notificationLabel.Text = ""; return; }
        var status = BuildDispatcherStatus(_selectedTrain);
        if (status.Contains("OCZEKIWANIE", StringComparison.OrdinalIgnoreCase))
        {
            var block = RailwayDispatcher.GetRequestedBlockName(_selectedTrain);
            var wait = RailwayDispatcher.GetWaitSeconds(_selectedTrain);
            _notificationLabel.Text = $"⚠ {_selectedTrain.Id.ToString()[..8]} czeka na trasę • blok {block} • {wait:0}s";
        }
        else if (status.Contains("PRZYZNANA", StringComparison.OrdinalIgnoreCase)) _notificationLabel.Text = $"✓ Trasa przyznana dla {_selectedTrain.Id.ToString()[..8]}";
        else if (_selectedTrain.LocomotiveScheduleRuntime?.DelaySeconds > 0) _notificationLabel.Text = $"Opóźnienie {_selectedTrain.Id.ToString()[..8]}: {FormatDelay(_selectedTrain.LocomotiveScheduleRuntime.DelaySeconds, true)}";
        else _notificationLabel.Text = status;
    }

    private static string BuildDispatcherStatus(Train train) => RailwayDispatcher.Current?.GetStatus(train) ?? "DISPATCHER NIEDOSTĘPNY";

    private static string BuildLocomotiveScheduleSummary(Train train)
    {
        var schedule = train.LocomotiveSchedule; var runtime = train.LocomotiveScheduleRuntime;
        if (schedule == null || !schedule.Enabled) return "Timetable lokomotywy: RĘCZ / wyłączony.";
        if (runtime == null || runtime.CurrentPointIndex < 0) return $"Timetable: {schedule.Name} • oczekiwanie na pierwszy punkt.";
        int current = Math.Clamp(runtime.CurrentPointIndex, 0, schedule.Points.Count - 1);
        int next = (current + 1) % schedule.Points.Count;
        var point = schedule.Points[current];
        int eta = runtime.GetExpectedArrival(schedule, next);
        return $"Timetable: {schedule.Name}\nPunkt: {current + 1}/{schedule.Points.Count}\nNastępny: {next + 1}/{schedule.Points.Count}\nETA: {FormatClock(eta)}\nPlanowany odjazd: {FormatClock(runtime.GetExpectedDeparture(schedule, current))}\nOpóźnienie: {FormatDelay(runtime.DelaySeconds, true)} • propagowane: {FormatDelay(runtime.PropagatedDelaySeconds, true)}\nStan: {runtime.State}";
    }

    private static string BuildModeText(TrainManager manager) => $"TRYB: NORMALNY • POCIĄGI {manager.Trains.Count} • BLOKI {manager.BlockController?.BlockCount ?? 0}";

    private void RebuildTools()
    {
        _toolContent.Widgets.Clear();
        AddTool("Tor prosty", TrackBuildMode.Straight); AddTool("Zakręt", TrackBuildMode.Curve); AddTool("Rozjazd", TrackBuildMode.Junction); AddTool("Semafor", TrackBuildMode.Signal); AddTool("Stacja", TrackBuildMode.Station); AddTool("Depot", TrackBuildMode.Depot);
        var route = CreateButton("TRASA WAGONU [S]", 260); route.Click += (_, _) => _toggleRouteEdit(); _toolContent.Widgets.Add(route);
        _toolContent.Visible = _toolsExpanded; _toolToggle.Content = new Label { Text = _toolsExpanded ? "NARZĘDZIA ▲" : "NARZĘDZIA ▼" };
    }

    private void AddTool(string text, TrackBuildMode mode) { var button = CreateButton(text, 260); button.Click += (_, _) => _setBuildMode(mode); _toolContent.Widgets.Add(button); }
    private static Label Header(string text) => new() { Text = text, Wrap = true };
    private static Grid CreateListGrid(int width) => new() { Width = width, HorizontalAlignment = HorizontalAlignment.Left, RowSpacing = 2 };
    private static void AddListButton(Grid grid, string text, Action action, string? tooltip = null) { var button = new Button { Content = new Label { Text = text, Wrap = true }, HorizontalAlignment = HorizontalAlignment.Stretch, Tooltip = tooltip }; button.Click += (_, _) => action(); Grid.SetRow(button, grid.Widgets.Count); grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); grid.Widgets.Add(button); }
    private static Button CreateButton(string text, int width) => new() { Content = new Label { Text = text, Wrap = true }, Width = width, HorizontalAlignment = HorizontalAlignment.Stretch };
    private static string FormatDelay(int seconds, bool hasSchedule) { if (!hasSchedule) return "—"; if (seconds == 0) return "0 s"; string sign = seconds > 0 ? "+" : "−"; int abs = Math.Abs(seconds); return $"{sign}{abs / 60}m {abs % 60:D2}s"; }
    private static string FormatClock(int seconds) { int normalized = ((seconds % 86400) + 86400) % 86400; return $"{normalized / 3600:D2}:{normalized / 60 % 60:D2}"; }
}
