using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>F10 infrastructure diagnostics: blocks, FCFS dispatcher queue, signals and safe dispatcher actions.</summary>
internal sealed class MyraRailwayDiagnosticsView
{
    public Widget Root { get; }
    private readonly TrainManager _manager;
    private readonly SignalController _signals;
    private readonly Action _close;
    private readonly VerticalStackPanel _content;

    public MyraRailwayDiagnosticsView(TrainManager manager, SignalController signals, Action close)
    {
        _manager = manager;
        _signals = signals;
        _close = close;
        var root = new VerticalStackPanel { Width = 1200, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Spacing = 6 };
        root.Widgets.Add(new Label { Text = "DIAGNOSTYKA RUCHU I INFRASTRUKTURY — F10", Wrap = true });
        root.Widgets.Add(new Label { Text = "Override omija wyłącznie arbitraż dispatchera. Nie zmienia zajętości bloku, sygnałów ani zwrotnic.", Wrap = true });
        var buttons = new HorizontalStackPanel { Spacing = 5 };
        var refresh = new Button { Content = new Label { Text = "ODŚWIEŻ" }, Width = 120 };
        refresh.Click += (_, _) => Refresh();
        var closeButton = new Button { Content = new Label { Text = "POWRÓT" }, Width = 120 };
        closeButton.Click += (_, _) => _close();
        buttons.Widgets.Add(refresh); buttons.Widgets.Add(closeButton); root.Widgets.Add(buttons);
        _content = new VerticalStackPanel { Spacing = 4 };
        root.Widgets.Add(new ScrollViewer { Height = 760, Content = _content });
        Root = root;
        Refresh();
    }

    private void Refresh()
    {
        _content.Widgets.Clear();
        var dispatcher = RailwayDispatcher.Current;
        _content.Widgets.Add(new Label { Text = $"POCIĄGI: {_manager.Trains.Count} • BLOKI: {_manager.BlockController?.BlockCount ?? 0} • KOLEJKA FCFS: {dispatcher?.PendingRequestCount ?? 0}", Wrap = true });

        _content.Widgets.Add(new Label { Text = "KOLEJKA ŻĄDAŃ — FCFS", Wrap = true });
        if (dispatcher == null || dispatcher.PendingRequests.Count == 0)
            _content.Widgets.Add(new Label { Text = "pusta" });
        else
        {
            foreach (var request in dispatcher.PendingRequests)
            {
                var train = _manager.Trains.FirstOrDefault(t => t.Id == request.TrainId);
                string wait = dispatcher == null ? "0.0s" : $"{Math.Max(0, (DateTime.UtcNow - request.RequestedAtUtc).TotalSeconds):0.0}s";
                string trainId = request.TrainId.ToString()[..8];
                string blockName = _manager.BlockController?.Blocks.FirstOrDefault(b => b.Id == request.BlockId)?.Name ?? request.BlockId.ToString()[..8];
                _content.Widgets.Add(new Label { Text = $"{trainId} → {blockName} • oczekiwanie {wait} • {(train == null ? "brak pociągu" : dispatcher.GetStatus(train))}", Wrap = true });
            }
        }

        _content.Widgets.Add(new Label { Text = "BLOKI", Wrap = true });
        if (_manager.BlockController != null)
        {
            foreach (var block in _manager.BlockController.Blocks)
            {
                string state = block.IsOccupied ? "OCCUPIED" : block.IsReserved ? "RESERVED" : block.IsCoolingDown ? $"FREE / COOLDOWN {block.CoolDownRemaining:0.0}s" : "FREE";
                string owner = block.OccupyingTrain != null ? $" zajmuje={block.OccupyingTrain.Id.ToString()[..8]}" : block.ReservedFor != null ? $" rezerwuje={block.ReservedFor.Id.ToString()[..8]}" : string.Empty;
                string signals = $"wejście={block.EntrySignal?.Name ?? "—"}/{block.EntrySignal?.Aspect.ToString() ?? "—"} wyjście={block.ExitSignal?.Name ?? "—"}/{block.ExitSignal?.Aspect.ToString() ?? "—"}";
                var row = new HorizontalStackPanel { Spacing = 4 };
                row.Widgets.Add(new Label { Text = $"{block.Name}: {state}{owner} • {signals}", Wrap = true, Width = 900 });
                if (block.ReservedFor != null)
                {
                    var release = new Button { Content = new Label { Text = "ZWOLNIJ REZERWACJĘ" }, Width = 180 };
                    var reservedTrain = block.ReservedFor;
                    release.Click += (_, _) => { RailwayDispatcher.NotifyReleased(reservedTrain); Refresh(); };
                    row.Widgets.Add(release);
                }
                _content.Widgets.Add(row);
            }
        }

        _content.Widgets.Add(new Label { Text = "SEMAFORY", Wrap = true });
        foreach (var signal in _signals.GetAllSignals())
        {
            var block = _manager.BlockController?.GetBlockForSignal(signal);
            string blockState = block == null ? "brak bloku" : block.IsOccupied ? "OCCUPIED" : block.IsReserved ? "RESERVED" : block.IsCoolingDown ? "COOLDOWN" : "FREE";
            _content.Widgets.Add(new Label { Text = $"{signal.Name}: aspekt={signal.Aspect} • blok={blockState}", Wrap = true });
        }

        _content.Widgets.Add(new Label { Text = "TIMETABLE / STAN POCIĄGÓW", Wrap = true });
        foreach (var train in _manager.Trains)
        {
            var schedule = train.LocomotiveSchedule;
            var runtime = train.LocomotiveScheduleRuntime;
            string scheduleState = schedule == null || !schedule.Enabled ? "RĘCZNE" : runtime == null || runtime.CurrentPointIndex < 0 ? "AUTO / START" : $"AUTO / {runtime.State} / pkt {runtime.CurrentPointIndex + 1}/{schedule.Points.Count} / delay {runtime.DelaySeconds:+#;-#;0}s / expected+{runtime.PropagatedDelaySeconds}s";
            var row = new HorizontalStackPanel { Spacing = 4 };
            row.Widgets.Add(new Label { Text = $"{train.Id.ToString()[..8]} • {scheduleState} • dispatcher={dispatcher?.GetStatus(train) ?? "—"}", Wrap = true, Width = 760 });
            var force = new Button { Content = new Label { Text = "WYMUSZ PRZEJAZD [F6]" }, Width = 190 };
            force.Click += (_, _) => { RailwayDispatcher.ForceProceed(train); Refresh(); };
            var release = new Button { Content = new Label { Text = "ZWOLNIJ TRASĘ" }, Width = 140 };
            release.Click += (_, _) => { RailwayDispatcher.NotifyReleased(train); Refresh(); };
            row.Widgets.Add(force); row.Widgets.Add(release);
            _content.Widgets.Add(row);
        }
    }
}
