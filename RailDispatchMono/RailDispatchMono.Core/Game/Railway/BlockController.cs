// ============================================================
// BLOCKCONTROLLER.CS - KONTROLER BLOKÓW TOROWYCH
// ============================================================

using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway
{
    /// <summary>
    /// Kontroler zarządzający wszystkimi blokami torowymi
    /// </summary>
    public class BlockController
    {
        // ============================================================
        // DANE
        // ============================================================

        private readonly List<Block> _blocks = new();
        private readonly Dictionary<MapPosition, Block> _cellToBlock = new();

        // ============================================================
        // REFERENCJE
        // ============================================================

        private GameMap? _map;
        private TrainManager? _trainManager;
        private SignalController? _signalController;

        // ============================================================
        // WŁAŚCIWOŚCI
        // ============================================================

        public IReadOnlyList<Block> Blocks => _blocks;
        public int BlockCount => _blocks.Count;
        public bool IsInitialized { get; private set; }

        // ============================================================
        // INICJALIZACJA
        // ============================================================

        public void Initialize(GameMap map, TrainManager trainManager, SignalController signalController)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _trainManager = trainManager ?? throw new ArgumentNullException(nameof(trainManager));
            _signalController = signalController ?? throw new ArgumentNullException(nameof(signalController));

            IsInitialized = true;

            DebugManager.Log("[BLOCK_CONTROLLER] Zainicjalizowano");
        }

        // ============================================================
        // TWORZENIE BLOKÓW Z SEMAFORÓW
        // ============================================================

        public void CreateBlocksFromSignals()
        {
            if (!IsInitialized)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] ? Nie zainicjalizowano!");
                return;
            }

            if (_signalController == null)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] ? Brak SignalController!");
                return;
            }

            if (_map == null)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] ? Brak mapy!");
                return;
            }

            ClearBlocks();
            DebugManager.Log("[BLOCK_CONTROLLER] Tworzenie bloków z semaforów...");

            var allSignals = _signalController.GetAllSignals();
            if (allSignals.Count == 0)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] ?? Brak semaforów - nie można utworzyć bloków");
                return;
            }

            DebugManager.Log($"[BLOCK_CONTROLLER] Znaleziono {allSignals.Count} semaforów");

            var sortedSignals = allSignals.OrderBy(s => s.Position.X)
                                          .ThenBy(s => s.Position.Y)
                                          .ToList();

            var visitedSignals = new HashSet<Guid>();
            Block? previousBlock = null;

            foreach (var startSignal in sortedSignals)
            {
                if (visitedSignals.Contains(startSignal.Id))
                    continue;

                var path = FindPathFromSignal(startSignal, visitedSignals);

                if (path != null && path.Count > 0)
                {
                    var endSignal = FindSignalAtPosition(path[^1]);
                    if (endSignal != null && !visitedSignals.Contains(endSignal.Id))
                    {
                        var block = new Block();
                        block.TrackCells.AddRange(path);
                        block.EntrySignal = startSignal;
                        block.ExitSignal = endSignal;
                        block.SetLength(CalculateBlockLength(block));

                        if (previousBlock != null)
                        {
                            previousBlock.NextBlock = block;
                            block.PreviousBlock = previousBlock;
                        }

                        AddBlock(block);
                        visitedSignals.Add(startSignal.Id);
                        visitedSignals.Add(endSignal.Id);
                        previousBlock = block;

                        DebugManager.Log($"[BLOCK_CONTROLLER] ? Utworzono blok: {startSignal.GetAspectName()} ? {endSignal.GetAspectName()}, Komórek: {path.Count}, Długość: {block.Length:F2}");
                    }
                }
                else
                {
                    visitedSignals.Add(startSignal.Id);
                }
            }

            DebugManager.Log($"[BLOCK_CONTROLLER] ? Utworzono {_blocks.Count} bloków");
        }

        // ============================================================
        // ZNAJDOWANIE ŚCIEŻKI OD SEMAFORA
        // ============================================================

        private List<MapPosition>? FindPathFromSignal(Signal startSignal, HashSet<Guid> visitedSignals)
        {
            if (_map == null || startSignal == null)
                return null;

            var path = new List<MapPosition>();
            var currentPos = startSignal.Position;
            var direction = startSignal.Direction;
            var visitedCells = new HashSet<MapPosition>();
            int maxSteps = 1000;
            int steps = 0;

            path.Add(currentPos);
            visitedCells.Add(currentPos);

            while (steps < maxSteps)
            {
                steps++;

                var nextPos = GetNextCell(currentPos, direction);

                if (nextPos.X < 0 || nextPos.X >= _map.Size.Width ||
                    nextPos.Y < 0 || nextPos.Y >= _map.Size.Height)
                {
                    break;
                }

                if (!_map.TryGetTrack(nextPos, out var track) || track == null)
                {
                    break;
                }

                path.Add(nextPos);
                visitedCells.Add(nextPos);

                var signalAtPos = FindSignalAtPosition(nextPos);
                if (signalAtPos != null && signalAtPos.Id != startSignal.Id)
                {
                    return path;
                }

                var exits = track.GetAvailableDirections();
                var opposite = GetOppositeDirection(direction);
                var nextDirection = exits.FirstOrDefault(d => d != opposite);

                if (nextDirection == TrackConnections.None)
                {
                    break;
                }

                currentPos = nextPos;
                direction = nextDirection;
            }

            return null;
        }

        // ============================================================
        // AKTUALIZACJA
        // ============================================================

        public void Update(float deltaTime)
        {
            if (!IsInitialized)
                return;

            foreach (var block in _blocks)
            {
                block.UpdateCooldown(deltaTime);
            }

            UpdateOccupancy();
            UpdateSignals();
        }

        // ============================================================
        // AKTUALIZACJA ZAJĘTOŚCI
        // ============================================================

        public void UpdateOccupancy()
        {
            if (_trainManager == null)
                return;

            foreach (var block in _blocks)
            {
                block.ClearTrains();
            }

            foreach (var train in _trainManager.Trains)
            {
                var blocks = GetBlocksForTrain(train);
                foreach (var block in blocks)
                {
                    block.AddTrain(train);
                }
            }
        }

        // ============================================================
        // AKTUALIZACJA SEMAFORÓW
        // ============================================================

        public void UpdateSignals()
        {
            if (_signalController == null)
                return;

            foreach (var block in _blocks)
            {
                if (block.EntrySignal == null)
                    continue;

                // 1. Blok zajęty → NIE ZMIENIAJ
                if (block.IsOccupied)
                {
                    continue;
                }

                // 2. Cooldown → STOP
                if (block.IsCoolingDown)
                {
                    block.EntrySignal.SetAspect(SignalAspect.Stop);
                    continue;
                }

                // 3. Następny blok zajęty → WARNING
                if (block.NextBlock != null && block.NextBlock.IsOccupiedOrCoolingDown)
                {
                    block.EntrySignal.SetAspect(SignalAspect.Warning);
                    continue;
                }

                // 4. ✅ WOLNY → RESETUJ NA CLEAR
                block.EntrySignal.SetAspect(SignalAspect.Clear);
            }
        }

        // ============================================================
        // POBIERANIE BLOKÓW
        // ============================================================

        public Block? GetBlockAtPosition(Vector2 position)
        {
            foreach (var block in _blocks)
            {
                if (block.ContainsPosition(position))
                    return block;
            }
            return null;
        }

        public Block? GetBlockAtPosition(MapPosition position)
        {
            var worldPos = new Vector2(position.X + 0.5f, position.Y + 0.5f);
            return GetBlockAtPosition(worldPos);
        }

        public Block? GetBlockForSignal(Signal signal)
        {
            if (signal == null)
                return null;

            return _blocks.FirstOrDefault(b => b.EntrySignal == signal || b.ExitSignal == signal);
        }

        public List<Block> GetBlocksForTrain(Train.Train train)
        {
            var result = new List<Block>();

            if (train == null)
                return result;

            var headBlock = GetBlockAtPosition(train.Position);
            if (headBlock != null && !result.Contains(headBlock))
                result.Add(headBlock);

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var transform = train.GetVehicleTransform(i);
                var block = GetBlockAtPosition(transform.Position);
                if (block != null && !result.Contains(block))
                    result.Add(block);
            }

            var tailBlock = GetBlockAtPosition(train.GetLastVehiclePosition());
            if (tailBlock != null && !result.Contains(tailBlock))
                result.Add(tailBlock);

            return result;
        }

        // ============================================================
        // REZERWACJA BLOKÓW
        // ============================================================

        public bool ReserveBlock(Block block, Train.Train train)
        {
            if (block == null || train == null)
                return false;

            return block.TryReserve(train);
        }

        public void ReleaseBlock(Block block)
        {
            if (block == null)
                return;

            block.ReleaseReservation();
        }

        // ============================================================
        // OBLICZANIE DŁUGOŚCI BLOKU
        // ============================================================

        public float CalculateBlockLength(Block block)
        {
            if (block == null || _map == null)
                return 0f;

            float totalLength = 0f;

            foreach (var cell in block.TrackCells)
            {
                totalLength += GetCellLength(cell);
            }

            return totalLength;
        }

        public float GetCellLength(MapPosition cell)
        {
            if (_map == null)
                return 1.0f;

            if (!_map.TryGetTrack(cell, out var track) || track == null)
                return 0f;

            if (track.Geometry == TrackGeometry.Curve)
            {
                return MathF.PI / 2f;
            }
            else if (track.Geometry == TrackGeometry.Junction)
            {
                return 1.0f;
            }
            else
            {
                return 1.0f;
            }
        }

        // ============================================================
        // METODY POMOCNICZE
        // ============================================================

        private MapPosition GetNextCell(MapPosition current, TrackConnections direction)
        {
            return direction switch
            {
                TrackConnections.North => new MapPosition(current.X, current.Y - 1),
                TrackConnections.East => new MapPosition(current.X + 1, current.Y),
                TrackConnections.South => new MapPosition(current.X, current.Y + 1),
                TrackConnections.West => new MapPosition(current.X - 1, current.Y),
                _ => current
            };
        }

        private TrackConnections GetOppositeDirection(TrackConnections direction)
        {
            return direction switch
            {
                TrackConnections.North => TrackConnections.South,
                TrackConnections.East => TrackConnections.West,
                TrackConnections.South => TrackConnections.North,
                TrackConnections.West => TrackConnections.East,
                _ => TrackConnections.None
            };
        }

        private Signal? FindSignalAtPosition(MapPosition position)
        {
            if (_signalController == null)
                return null;

            var signals = _signalController.GetSignalsAt(position);
            return signals.FirstOrDefault();
        }

        private bool IsSignalPosition(MapPosition position)
        {
            if (_signalController == null)
                return false;

            var signals = _signalController.GetSignalsAt(position);
            return signals.Count > 0;
        }

        public void AddBlock(Block block)
        {
            if (block == null)
                return;

            _blocks.Add(block);

            foreach (var cell in block.TrackCells)
            {
                _cellToBlock[cell] = block;
            }
        }

        public void ClearBlocks()
        {
            _blocks.Clear();
            _cellToBlock.Clear();
        }

        public override string ToString()
        {
            return $"[BlockController] Blocks: {_blocks.Count}, Initialized: {IsInitialized}";
        }
    }
}