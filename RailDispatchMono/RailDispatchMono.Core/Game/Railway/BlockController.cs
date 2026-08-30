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
    public class BlockController
    {
        private readonly List<Block> _blocks = new();
        private readonly Dictionary<MapPosition, Block> _cellToBlock = new();
        private readonly Dictionary<Guid, bool> _previousOccupancy = new();

        private GameMap? _map;
        private TrainManager? _trainManager;
        private SignalController? _signalController;

        public IReadOnlyList<Block> Blocks => _blocks;
        public int BlockCount => _blocks.Count;
        public bool IsInitialized { get; private set; }

        public void Initialize(GameMap map, TrainManager trainManager, SignalController signalController)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _trainManager = trainManager ?? throw new ArgumentNullException(nameof(trainManager));
            _signalController = signalController ?? throw new ArgumentNullException(nameof(signalController));
            IsInitialized = true;
            DebugManager.Log("[BLOCK_CONTROLLER] Zainicjalizowano");
        }

        public void CreateBlocksFromSignals()
        {
            if (!IsInitialized || _signalController == null || _map == null)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] Nie można utworzyć bloków - brak inicjalizacji/mapy/semaforów");
                return;
            }

            ClearBlocks();
            DebugManager.Log("[BLOCK_CONTROLLER] Tworzenie bloków z semaforów...");

            var allSignals = _signalController.GetAllSignals();
            if (allSignals.Count == 0)
            {
                DebugManager.Log("[BLOCK_CONTROLLER] Brak semaforów - nie można utworzyć bloków");
                return;
            }

            var sortedSignals = allSignals.OrderBy(s => s.Position.X).ThenBy(s => s.Position.Y).ToList();
            var visitedSignals = new HashSet<Guid>();
            Block? previousBlock = null;

            foreach (var startSignal in sortedSignals)
            {
                if (visitedSignals.Contains(startSignal.Id))
                    continue;

                var path = FindPathFromSignal(startSignal, visitedSignals);
                if (path == null || path.Count == 0)
                {
                    visitedSignals.Add(startSignal.Id);
                    continue;
                }

                var endSignal = FindSignalAtPosition(path[^1]);
                if (endSignal == null || visitedSignals.Contains(endSignal.Id))
                    continue;

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
                _previousOccupancy[block.Id] = false;
                visitedSignals.Add(startSignal.Id);
                visitedSignals.Add(endSignal.Id);
                previousBlock = block;
            }

            DebugManager.Log($"[BLOCK_CONTROLLER] Utworzono {_blocks.Count} bloków");
        }

        private List<MapPosition>? FindPathFromSignal(Signal startSignal, HashSet<Guid> visitedSignals)
        {
            if (_map == null)
                return null;

            var path = new List<MapPosition>();
            var currentPos = startSignal.Position;
            var direction = startSignal.Direction;
            var visitedCells = new HashSet<MapPosition>();
            const int maxSteps = 1000;

            path.Add(currentPos);
            visitedCells.Add(currentPos);

            for (int steps = 0; steps < maxSteps; steps++)
            {
                var nextPos = GetNextCell(currentPos, direction);
                if (nextPos.X < 0 || nextPos.X >= _map.Size.Width || nextPos.Y < 0 || nextPos.Y >= _map.Size.Height)
                    break;
                if (!_map.TryGetTrack(nextPos, out var track) || track == null)
                    break;
                if (!visitedCells.Add(nextPos))
                    break;

                path.Add(nextPos);
                var signalAtPos = FindSignalAtPosition(nextPos);
                if (signalAtPos != null && signalAtPos.Id != startSignal.Id)
                    return path;

                var exits = track.GetAvailableDirections();
                var opposite = GetOppositeDirection(direction);
                var nextDirection = exits.FirstOrDefault(d => d != opposite);
                if (nextDirection == TrackConnections.None)
                    break;

                currentPos = nextPos;
                direction = nextDirection;
            }

            return null;
        }

        public void Update(float deltaTime)
        {
            if (!IsInitialized)
                return;

            UpdateOccupancy();

            foreach (var block in _blocks)
                block.UpdateCooldown(deltaTime);

            UpdateSignals();
        }

        public void UpdateOccupancy()
        {
            if (_trainManager == null)
                return;

            foreach (var block in _blocks)
            {
                bool wasOccupied = _previousOccupancy.TryGetValue(block.Id, out bool previous) && previous;
                block.ClearTrains();

                foreach (var train in _trainManager.Trains)
                {
                    foreach (var trainBlock in GetBlocksForTrain(train))
                    {
                        if (trainBlock == block)
                        {
                            block.AddTrain(train);
                            break;
                        }
                    }
                }

                bool isOccupied = block.IsOccupied;
                if (isOccupied)
                {
                    block.CancelCooldown();
                }
                else if (wasOccupied)
                {
                    block.StartCooldown();
                    DebugManager.Log($"[BLOCK] {block.Name} opuszczony - semafor pozostanie bez zmian przez {Block.CoolDownDuration:F1}s");
                }

                _previousOccupancy[block.Id] = isOccupied;
            }
        }

        /// <summary>
        /// Automatic signal control is deliberately one-way for now:
        /// only Clear -> Stop is automatic. Stop -> permissive aspects remain manual.
        /// </summary>
        public void UpdateSignals()
        {
            if (_signalController == null)
                return;

            foreach (var block in _blocks)
            {
                var signal = block.EntrySignal;
                if (signal == null || block.IsOccupied || block.IsCoolingDown)
                    continue;

                if (signal.Aspect == SignalAspect.Clear)
                {
                    if (signal.SetAspect(SignalAspect.Stop))
                        DebugManager.Log($"[SIGNAL] {signal.Name} automatycznie: Clear -> Stop po zwolnieniu bloku {block.Name}");
                }
            }
        }

        public Block? GetBlockAtPosition(Vector2 position) => _blocks.FirstOrDefault(block => block.ContainsPosition(position));

        public Block? GetBlockAtPosition(MapPosition position) => GetBlockAtPosition(new Vector2(position.X + 0.5f, position.Y + 0.5f));

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
            if (headBlock != null)
                result.Add(headBlock);

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var block = GetBlockAtPosition(train.GetVehicleTransform(i).Position);
                if (block != null && !result.Contains(block))
                    result.Add(block);
            }

            var tailBlock = GetBlockAtPosition(train.GetLastVehiclePosition());
            if (tailBlock != null && !result.Contains(tailBlock))
                result.Add(tailBlock);

            return result;
        }

        public bool ReserveBlock(Block block, Train.Train train) => block != null && train != null && block.TryReserve(train);

        public void ReleaseBlock(Block block)
        {
            if (block != null)
                block.ReleaseReservation();
        }

        public float CalculateBlockLength(Block block)
        {
            if (block == null || _map == null)
                return 0f;
            return block.TrackCells.Sum(GetCellLength);
        }

        public float GetCellLength(MapPosition cell)
        {
            if (_map == null || !_map.TryGetTrack(cell, out var track) || track == null)
                return 0f;
            return track.Geometry == TrackGeometry.Curve ? MathF.PI / 2f : 1.0f;
        }

        private MapPosition GetNextCell(MapPosition current, TrackConnections direction) => direction switch
        {
            TrackConnections.North => new MapPosition(current.X, current.Y - 1),
            TrackConnections.East => new MapPosition(current.X + 1, current.Y),
            TrackConnections.South => new MapPosition(current.X, current.Y + 1),
            TrackConnections.West => new MapPosition(current.X - 1, current.Y),
            _ => current
        };

        private TrackConnections GetOppositeDirection(TrackConnections direction) => direction switch
        {
            TrackConnections.North => TrackConnections.South,
            TrackConnections.East => TrackConnections.West,
            TrackConnections.South => TrackConnections.North,
            TrackConnections.West => TrackConnections.East,
            _ => TrackConnections.None
        };

        private Signal? FindSignalAtPosition(MapPosition position)
        {
            if (_signalController == null)
                return null;
            return _signalController.GetSignalsAt(position).FirstOrDefault();
        }

        private bool IsSignalPosition(MapPosition position) =>
            _signalController != null && _signalController.GetSignalsAt(position).Count > 0;

        public void AddBlock(Block block)
        {
            if (block == null)
                return;
            _blocks.Add(block);
            foreach (var cell in block.TrackCells)
                _cellToBlock[cell] = block;
        }

        public void ClearBlocks()
        {
            _blocks.Clear();
            _cellToBlock.Clear();
            _previousOccupancy.Clear();
        }

        public override string ToString() => $"[BlockController] Blocks: {_blocks.Count}, Initialized: {IsInitialized}";
    }
}
