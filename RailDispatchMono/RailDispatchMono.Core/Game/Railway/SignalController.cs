// ============================================================
// SIGNALCONTROLLER.CS - KONTROLER SEMAFORÓW
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway
{
    public class SignalController
    {
        private readonly Dictionary<MapPosition, List<Signal>> _signals = new();
        private readonly Dictionary<Guid, Signal> _signalById = new();
        private readonly GameMap _map;

        public SignalController(GameMap map) => _map = map ?? throw new ArgumentNullException(nameof(map));
        public IReadOnlyDictionary<MapPosition, List<Signal>> Signals => _signals;

        public bool HasSignalAt(MapPosition position, TrackConnections? direction = null)
        {
            if (!_signals.TryGetValue(position, out var signals)) return false;
            return direction.HasValue ? signals.Any(s => s.Direction == direction.Value) : signals.Count > 0;
        }

        public bool AddSignal(MapPosition position, TrackConnections direction, List<SignalAspect>? availableAspects = null) =>
            AddSignal(new Signal(position, direction, availableAspects));

        public bool AddSignal(Signal signal)
        {
            if (signal == null || HasSignalAt(signal.Position, signal.Direction)) return false;
            if (!_map.TryGetTrack(signal.Position, out var track) || track == null) return false;
            if (!track.HasConnection(signal.Direction)) return false;
            if (_signalById.ContainsKey(signal.Id)) return false;
            if (!_signals.ContainsKey(signal.Position)) _signals[signal.Position] = new List<Signal>();
            _signals[signal.Position].Add(signal);
            _signalById[signal.Id] = signal;
            return true;
        }

        public void RemoveSignalsAt(MapPosition position)
        {
            if (!_signals.TryGetValue(position, out var signalList)) return;
            foreach (var signal in signalList) _signalById.Remove(signal.Id);
            signalList.Clear();
            _signals.Remove(position);
        }

        public void Clear()
        {
            _signals.Clear();
            _signalById.Clear();
        }

        public List<Signal> GetSignalsAt(MapPosition position) =>
            _signals.TryGetValue(position, out var signals) ? signals.ToList() : new List<Signal>();

        public Signal? GetSignalAt(MapPosition position, TrackConnections direction) =>
            _signals.TryGetValue(position, out var signals) ? signals.FirstOrDefault(s => s.Direction == direction) : null;

        public List<Signal> GetAllSignals() => _signals.Values.SelectMany(x => x).ToList();
    }
}