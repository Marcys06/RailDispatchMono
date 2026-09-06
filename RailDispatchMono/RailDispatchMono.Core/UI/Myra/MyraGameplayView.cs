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

/// <summary>0.2.6 player-facing operational HUD. World actions remain owned by the gameplay input layer.</summary>
internal sealed class MyraGameplayView
{
    public Widget Root { get; }
    private Label _clock;
    private Label _status;
    private Label _mode;
    private Label _selection;
    private Label _selectionDetails;
    private Label _dispatcher;
    private Label _infrastructure;
    private Grid _trains;
    private Grid _stations;
    private Grid _wagons;
    private VerticalStackPanel _tools;
    private Button _toolsToggle;
    private bool _toolsOpen;
    private Train? _selectedTrain;
    private readonly Action<float> _setSpeed;
    private readonly Action<Train> _focusTrain;
    private readonly Action<Station> _focusStation;
    private readonly Action<TrackBuildMode> _setBuildMode;
    private readonly Action _toggleRouteEdit;
    private readonly Action<Train> _forceProceed;
    private readonly Action<Train> _releaseRoute;
    private readonly Action<Train> _toggleAutomation;

    public MyraGameplayView(Action<float> setSpeed, Action<Train> focusTrain, Action<Station> focusStation,
        Action<TrackBuildMode> setBuildMode, Action toggleRouteEdit, Action<Train> forceProceed,
        Action<Train> releaseRoute, Action<Train> toggleAutomation)
    {
        _setSpeed = setSpeed;
        _focusTrain = train => { _selectedTrain = train; focusTrain(train); Refresh(); };
        _focusStation = focusStation;
        _setBuildMode = setBuildMode;
        _toggleRouteEdit = toggleRouteEdit;
        _forceProceed = forceProceed;
        _releaseRoute = releaseRoute;
        _toggleAutomation = toggleAutomation;

        var root = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, RowSpacing = 5, ColumnSpacing = 6 };
        root.RowsProportions.Add(new Proportion(ProportionType.Auto));
        root.RowsProportions.Add(new Proportion(ProportionType.Part, 1));
        root.RowsProportions.Add(new Proportion(ProportionType.Auto));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 300));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        root.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 360));

        var top = BuildTop(); Grid.SetRow(top, 0); Grid.SetColumnSpan(top, 3); root.Widgets.Add(top);
        var left = BuildTools(); Grid.SetRow(left, 1); Grid.SetColumn(left, 0); root.Widgets.Add(left);
        var center = BuildSelection(); Grid.SetRow(center, 1); Grid.SetColumn(center, 1); root.Widgets.Add(center);
        var right = BuildTraffic(); Grid.SetRow(right, 1); Grid.SetColumn(right, 2); root.Widgets.Add(right);
        var bottom = BuildBottom(); Grid.SetRow(bottom, 2); Grid.SetColumnSpan(bottom, 3); root.Widgets.Add(bottom);
        Root = root;
        Refresh();
    }

    public void Refresh()
    {
        var clock = GameClock.Current;
        _clock.Text = clock?.DisplayTime ?? "--:--";
        _status.Text = clock == null ? "BRAK SESJI" : $"DZIEŃ {clock.GameDay} • SYMULACJA x{clock.SimulationSpeed:0}";
        var manager = TrainManager.Current;
        if (manager == null) return;
        _mode.Text = BuildModeText();
        _infrastructure.Text = $"BLOKI {manager.BlockController?.BlockCount ?? 0} • POCIĄGI {manager.Trains.Count}";
        RebuildTraffic(manager);
        RefreshSelection();
    }

    private Widget BuildTop()
    {
        var bar = new HorizontalStackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Stretch };
        _clock = new Label { Text = "--:--", Width = 85 };
        _status = new Label { Text = "BRAK SESJI", Width = 230, Wrap = true };
        _mode = new Label { Text = "TRYB: ZAZNACZANIE", Width = 300, Wrap = true };
        _infrastructure = new Label { Text = "BLOKI — • POCIĄGI —", Width = 210, Wrap = true };
        bar.Widgets.Add(_clock); bar.Widgets.Add(_status); bar.Widgets.Add(_mode); bar.Widgets.Add(_infrastructure);
        foreach (var item in new[] { ("1x", 1f), ("2x", 2f), ("5x", 5f) })
        {
            var b = Button(item.Item1, 48); b.Click += (_, _) => _setSpeed(item.Item2); bar.Widgets.Add(b);
        }
        return bar;
    }

    private Widget BuildTools()
    {
        var panel = new VerticalStackPanel { Width = 290, Spacing = 4 };
        panel.Widgets.Add(Header("STEROWANIE INFRASTRUKTURĄ"));
        var select = Button("ZAZNACZANIE / BRAK TRYBU [0]", 285);
        select.Click += (_, _) => _setBuildMode(TrackBuildMode.None);
        panel.Widgets.Add(select);
        _toolsToggle = Button("NARZĘDZIA BUDOWY ▼", 285);
        _toolsToggle.Click += (_, _) => { _toolsOpen = !_toolsOpen; RebuildTools(); };
        panel.Widgets.Add(_toolsToggle);
        _tools = new VerticalStackPanel { Width = 285, Spacing = 2, Visible = false };
        panel.Widgets.Add(_tools);
        panel.Widgets.Add(Header("OPERACJE"));
        var route = Button("TRASA WAGONU [S]", 285); route.Click += (_, _) => _toggleRouteEdit(); panel.Widgets.Add(route);
        var f8 = Button("INFRASTRUKTURA TORU [F8]", 285); f8.Click += (_, _) => _setBuildMode(TrackBuildMode.None); panel.Widgets.Add(f8);
        panel.Widgets.Add(new Label { Text = "Zaznaczanie: LPM = nowa selekcja • Shift+LPM = dodaj • Ctrl+LPM = przełącz.", Wrap = true });
        return panel;
    }

    private void RebuildTools()
    {
        _tools.Widgets.Clear();
        AddTool("1  Tor prosty", TrackBuildMode.Straight);
        AddTool("2  Zakręt", TrackBuildMode.Curve);
        AddTool("3  Rozjazd", TrackBuildMode.Junction);
        AddTool("4  Semafor", TrackBuildMode.Signal);
        AddTool("5  Stacja", TrackBuildMode.Station);
        AddTool("9  Depot", TrackBuildMode.Depot);
        _tools.Visible = _toolsOpen;
        _toolsToggle.Content = new Label { Text = _toolsOpen ? "NARZĘDZIA BUDOWY ▲" : "NARZĘDZIA BUDOWY ▼" };
    }

    private void AddTool(string text, TrackBuildMode mode)
    {
        var b = Button(text, 285); b.Click += (_, _) => _setBuildMode(mode); _tools.Widgets.Add(b);
    }

    private Widget BuildSelection()
    {
        var panel = new VerticalStackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
        panel.Widgets.Add(Header("WYBRANY POCIĄG"));
        _selection = new Label { Text = "Nie wybrano pociągu.", Wrap = true };
        _selectionDetails = new Label { Text = "Wybierz skład z panelu RUCH.", Wrap = true };
        panel.Widgets.Add(_selection); panel.Widgets.Add(_selectionDetails);
        var actions = new HorizontalStackPanel { Spacing = 4 };
        var force = Button("WYMUSZENIE", 120); force.Click += (_, _) => { if (_selectedTrain != null) _forceProceed(_selectedTrain); };
        var release = Button("ZWOLNIJ", 100); release.Click += (_, _) => { if (_selectedTrain != null) _releaseRoute(_selectedTrain); };
        var auto = Button("AUTO / RĘCZ", 125); auto.Click += (_, _) => { if (_selectedTrain != null) _toggleAutomation(_selectedTrain); };
        actions.Widgets.Add(force); actions.Widgets.Add(release); actions.Widgets.Add(auto); panel.Widgets.Add(actions);
        panel.Widgets.Add(Header("ZASADA RUCHU"));
        panel.Widgets.Add(new Label { Text = "FCFS: pierwszy zgłoszony pociąg wygrywa konflikt o blok. F6 omija arbitraż, ale nie zajętość bloku. RadioStop ma pierwszeństwo.", Wrap = true });
        return panel;
    }

    private Widget BuildTraffic()
    {
        var panel = new VerticalStackPanel { Width = 355, Spacing = 4 };
        panel.Widgets.Add(Header("RUCH"));
        _dispatcher = new Label { Text = "Dyspozytor —", Wrap = true }; panel.Widgets.Add(_dispatcher);
        _trains = ListGrid(350); panel.Widgets.Add(Header("POCIĄGI")); panel.Widgets.Add(new ScrollViewer { Height = 250, Content = _trains });
        _stations = ListGrid(350); panel.Widgets.Add(Header("STACJE")); panel.Widgets.Add(new ScrollViewer { Height = 105, Content = _stations });
        return panel;
    }

    private Widget BuildBottom()
    {
        var panel = new HorizontalStackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Stretch };
        var wag = new VerticalStackPanel { Width = 650, Spacing = 2 };
        wag.Widgets.Add(Header("WAGONY / PASAŻEROWIE"));
        _wagons = ListGrid(640); wag.Widgets.Add(new ScrollViewer { Height = 85, Content = _wagons }); panel.Widgets.Add(wag);
        var help = new VerticalStackPanel { Width = 650, Spacing = 2 };
        help.Widgets.Add(Header("SKRÓTY"));
        help.Widgets.Add(new Label { Text = "0 = zaznaczanie • 1–5/9 = budowa • F6 wymuś przejazd • F7 kierunek • F8 infrastruktura • F9 rozkład • F10 diagnostyka • F11 linie kolejowe • C/X sprzęganie/rozprzęganie.", Wrap = true });
        panel.Widgets.Add(help);
        return panel;
    }

    private void RebuildTraffic(TrainManager manager)
    {
        _trains.Widgets.Clear(); _trains.RowsProportions.Clear();
        _stations.Widgets.Clear(); _stations.RowsProportions.Clear();
        _wagons.Widgets.Clear(); _wagons.RowsProportions.Clear();
        int auto = 0;
        foreach (var train in manager.Trains.Take(16))
        {
            bool automatic = train.LocomotiveSchedule?.Enabled == true;
            if (automatic) auto++;
            string marker = automatic ? "AUTO" : "RĘCZ";
            string delay = train.LocomotiveScheduleRuntime == null ? "—" : FormatDelay(train.LocomotiveScheduleRuntime.DelaySeconds);
            string selected = _selectedTrain?.Id == train.Id ? " ◀" : "";
            Add(_trains, $"{marker}  {train.Id.ToString()[..6]}  {train.Speed * 3.6f:0} km/h  {delay}{selected}", () => _focusTrain(train));
        }
        _dispatcher.Text = $"POCIĄGI {manager.Trains.Count} • AUTO {auto} • {_selectedTrain?.Id.ToString()[..6] ?? "—"}\n{(_selectedTrain == null ? "Wybierz skład." : RailwayDispatcher.Current?.GetStatus(_selectedTrain) ?? "Dyspozytor —")}";
        foreach (var station in manager.StationController.Stations.Take(6))
        {
            int waiting = manager.StationController.Passengers.GetWaitingCount(station);
            Add(_stations, $"{station.Name} • oczekuje {waiting}", () => _focusStation(station));
        }
        foreach (var train in manager.Trains)
        {
            foreach (var wagon in train.Composition.Vehicles.OfType<Wagon>())
            {
                Add(_wagons, $"{wagon.ShortName} • {wagon.PassengerCount}/{wagon.PassengerCapacity} • {FormatDelay(wagon.ScheduleRuntime.DelaySeconds)}", () => _focusTrain(train));
                if (_wagons.Widgets.Count >= 6) break;
            }
            if (_wagons.Widgets.Count >= 6) break;
        }
    }

    private void RefreshSelection()
    {
        if (_selectedTrain == null || TrainManager.Current == null || !TrainManager.Current.Trains.Contains(_selectedTrain))
        {
            _selection.Text = "Nie wybrano pociągu."; _selectionDetails.Text = "Wybierz skład z panelu RUCH."; return;
        }
        var train = _selectedTrain;
        string mode = train.LocomotiveSchedule?.Enabled == true ? "AUTO" : "RĘCZ";
        string radio = train.IsRadioStopped ? " • RADIOSTOP" : "";
        _selection.Text = $"{train.Id.ToString()[..8]} • {mode} • {train.Speed * 3.6f:0.0} km/h{radio}";
        _selectionDetails.Text = $"Vmax składu: {train.MaxSpeed * 3.6f:0.0} km/h\nCel: {train.EffectiveTargetSpeed * 3.6f:0.0} km/h\nKierunek: {train.Direction}\nDispatcher: {RailwayDispatcher.Current?.GetStatus(train) ?? "—"}\nTimetable: {BuildSchedule(train)}";
    }

    private static string BuildSchedule(Train train)
    {
        var s = train.LocomotiveSchedule; var r = train.LocomotiveScheduleRuntime;
        if (s == null || !s.Enabled) return "RĘCZ / wyłączony";
        if (r == null || s.Points.Count == 0) return s.Name;
        return $"{s.Name} • punkt {Math.Clamp(r.CurrentPointIndex + 1, 1, s.Points.Count)}/{s.Points.Count} • opóźn. {FormatDelay(r.DelaySeconds)} • {r.State}";
    }

    private string BuildModeText()
    {
        return "TRYB: ZAZNACZANIE";
    }

    private static Grid ListGrid(int width) => new() { Width = width, HorizontalAlignment = HorizontalAlignment.Left, RowSpacing = 2 };
    private static void Add(Grid grid, string text, Action action)
    {
        var b = Button(text, grid.Width - 5); b.Click += (_, _) => action(); Grid.SetRow(b, grid.Widgets.Count); grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); grid.Widgets.Add(b);
    }
    private static Label Header(string text) => new() { Text = text, Wrap = true };
    private static Button Button(string text, int width) => new() { Content = new Label { Text = text, Wrap = true }, Width = width, HorizontalAlignment = HorizontalAlignment.Stretch };
    private static string FormatDelay(int seconds) { if (seconds == 0) return "0s"; string sign = seconds > 0 ? "+" : "−"; int a = Math.Abs(seconds); return $"{sign}{a / 60}m {a % 60:D2}s"; }
}
