using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>Full infrastructure diagnostics: blocks, dispatcher queue and signals.</summary>
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
        root.Widgets.Add(new Label { Text = "DIAGNOSTYKA RUCHU I INFRASTRUKTURY", Wrap = true });
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
        _content.Widgets.Add(new Label { Text = $"POCIĄGI: {_manager.Trains.Count} • BLOKI: {_manager.BlockController?.BlockCount ?? 0} • KOLEJKA DISPATCHERA: {dispatcher?.PendingRequestCount ?? 0}", Wrap = true });
        _content.Widgets.Add(new Label { Text = "KOLEJKA ŻĄDAŃ — FCFS", Wrap = true });
        if (dispatcher == null || dispatcher.PendingRequestTrainIds.Count == 0)
            _content.Widgets.Add(new Label { Text = "pusta" });
        else
            _content.Widgets.Add(new Label { Text = string.Join(" → ", dispatcher.PendingRequestTrainIds.Select(id => id.ToString()[..8])), Wrap = true });

        _content.Widgets.Add(new Label { Text = "BLOKI", Wrap = true });
        if (_manager.BlockController != null)
        {
            foreach (var block in _manager.BlockController.Blocks)
            {
                string state = block.IsOccupied ? "OCCUPIED" : block.IsReserved ? "RESERVED" : block.IsCoolingDown ? $"FREE ({block.CoolDownRemaining:0.0}s cooldown)" : "FREE";
                string owner = block.OccupyingTrain != null ? $" train={block.OccupyingTrain.Id.ToString()[..8]}" : block.ReservedFor != null ? $" reserved={block.ReservedFor.Id.ToString()[..8]}" : string.Empty;
                string signals = $"wejście={block.EntrySignal?.Name ?? "—"}/{block.EntrySignal?.Aspect.ToString() ?? "—"} wyjście={block.ExitSignal?.Name ?? "—"}/{block.ExitSignal?.Aspect.ToString() ?? "—"}";
                _content.Widgets.Add(new Label { Text = $"{block.Name}: {state}{owner} • {signals}", Wrap = true });
            }
        }

        _content.Widgets.Add(new Label { Text = "SEMAFORY", Wrap = true });
        foreach (var signal in _signals.GetAllSignals())
        {
            var block = _manager.BlockController?.GetBlockForSignal(signal);
            string blockState = block == null ? "brak bloku" : block.IsOccupied ? "OCCUPIED" : block.IsReserved ? "RESERVED" : block.IsCoolingDown ? "COOLDOWN" : "FREE";
            _content.Widgets.Add(new Label { Text = $"{signal.Name}: {signal.Aspect} • blok={blockState}", Wrap = true });
        }

        _content.Widgets.Add(new Label { Text = "TIMETABLE / STAN POCIĄGÓW", Wrap = true });
        foreach (var train in _manager.Trains)
        {
            var schedule = train.LocomotiveSchedule;
            var runtime = train.LocomotiveScheduleRuntime;
            string scheduleState = schedule == null || !schedule.Enabled ? "RĘCZNE" : runtime == null || runtime.CurrentPointIndex < 0 ? "AUTO / START" : $"AUTO / {runtime.State} / pkt {runtime.CurrentPointIndex + 1}/{schedule.Points.Count} / delay {runtime.DelaySeconds:+#;-#;0}s / expected+{runtime.PropagatedDelaySeconds}s";
            _content.Widgets.Add(new Label { Text = $"{train.Id.ToString()[..8]} • {scheduleState} • dispatcher={dispatcher?.GetStatus(train) ?? "—"}", Wrap = true });
        }
    }
}
