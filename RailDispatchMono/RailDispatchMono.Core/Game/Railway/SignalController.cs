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

        public SignalController(GameMap map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        // ============================================================
        // W£AŒCIWOŒCI
        // ============================================================
        public IReadOnlyDictionary<MapPosition, List<Signal>> Signals => _signals;

        // ============================================================
        // METODY
        // ============================================================

        public bool HasSignalAt(MapPosition position, TrackConnections? direction = null)
        {
            if (!_signals.TryGetValue(position, out var signals))
                return false;

            if (direction.HasValue)
                return signals.Any(s => s.Direction == direction.Value);

            return signals.Count > 0;
        }

        public bool AddSignal(MapPosition position, TrackConnections direction, List<SignalAspect>? availableAspects = null)
        {
            if (HasSignalAt(position, direction))
                return false;

            if (!_map.TryGetTrack(position, out var track) || track == null)
                return false;

            if (!track.HasConnection(direction))
                return false;

            var signal = new Signal(position, direction, availableAspects);

            if (!_signals.ContainsKey(position))
                _signals[position] = new List<Signal>();

            _signals[position].Add(signal);
            _signalById[signal.Id] = signal;

            return true;
        }

        public void RemoveSignalsAt(MapPosition position)
        {
            if (_signals.TryGetValue(position, out var signalList))
            {
                signalList.Clear();
                _signals.Remove(position);
// DebugManager.Log($"[SIGNAL] Usuniêto semafory na {position}");
            }
        }

        public List<Signal> GetSignalsAt(MapPosition position)
        {
            return _signals.TryGetValue(position, out var signals)
                ? signals.ToList()
                : new List<Signal>();
        }

        public Signal? GetSignalAt(MapPosition position, TrackConnections direction)
        {
            if (!_signals.TryGetValue(position, out var signals))
                return null;

            return signals.FirstOrDefault(s => s.Direction == direction);
        }


        public List<Signal> GetAllSignals()
        {
            var result = new List<Signal>();
            foreach (var kvp in _signals)
            {
                result.AddRange(kvp.Value);
            }
            return result;
        }
    }

}