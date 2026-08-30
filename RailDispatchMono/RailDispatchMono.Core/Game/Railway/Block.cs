// ============================================================
// BLOCK.CS - REPREZENTACJA BLOKU TOROWEGO
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Railway
{
    /// <summary>
    /// Blok torowy - odcinek toru między dwoma semaforami.
    /// </summary>
    public class Block
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public List<MapPosition> TrackCells { get; }
        public List<Vector2> Waypoints { get; }

        public Signal? EntrySignal { get; set; }
        public Signal? ExitSignal { get; set; }

        private readonly List<Train.Train> _trainsOnBlock = new();
        public IReadOnlyList<Train.Train> TrainsOnBlock => _trainsOnBlock;
        public bool IsOccupied => _trainsOnBlock.Count > 0;
        public Train.Train? OccupyingTrain => _trainsOnBlock.FirstOrDefault();

        private float _coolDownTimer;
        private bool _isCoolingDown;
        private const float CoolDownDuration = 3.0f;

        public bool IsCoolingDown => _isCoolingDown;
        public bool IsOccupiedOrCoolingDown => IsOccupied || IsCoolingDown;

        public bool IsReserved { get; private set; }
        public Train.Train? ReservedFor { get; private set; }

        public float Length { get; private set; }
        public float LengthInMeters => Length * 100f;

        public Block? PreviousBlock { get; set; }
        public Block? NextBlock { get; set; }
        public List<Block> AlternativeNextBlocks { get; }

        public Block()
        {
            Id = Guid.NewGuid();
            Name = $"Block_{Id.ToString()[..8]}";
            TrackCells = new List<MapPosition>();
            Waypoints = new List<Vector2>();
            AlternativeNextBlocks = new List<Block>();
        }

        public bool ContainsPosition(Vector2 position)
        {
            foreach (var cell in TrackCells)
            {
                var cellPos = new Vector2(cell.X + 0.5f, cell.Y + 0.5f);
                if (Vector2.Distance(position, cellPos) < 0.55f)
                    return true;
            }
            return false;
        }

        public bool ContainsCell(MapPosition cell) => TrackCells.Contains(cell);

        public void AddTrain(Train.Train train)
        {
            if (!_trainsOnBlock.Contains(train))
                _trainsOnBlock.Add(train);
        }

        public void RemoveTrain(Train.Train train) => _trainsOnBlock.Remove(train);
        public bool ContainsTrain(Train.Train train) => _trainsOnBlock.Contains(train);
        public void ClearTrains() => _trainsOnBlock.Clear();

        public void StartCooldown()
        {
            _coolDownTimer = CoolDownDuration;
            _isCoolingDown = true;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (!_isCoolingDown)
                return;

            _coolDownTimer -= deltaTime;
            if (_coolDownTimer <= 0f)
            {
                _coolDownTimer = 0f;
                _isCoolingDown = false;
            }
        }

        public void OnTrainExited(Train.Train train)
        {
            RemoveTrain(train);
            if (!IsOccupied)
                StartCooldown();
        }

        public void ResetEntrySignals()
        {
            // Intentionally does not change the aspect. Signal release is manual.
        }

        public bool IsTrainFullyOnBlock(Train.Train train)
        {
            return ContainsPosition(train.Position) && ContainsPosition(train.GetLastVehiclePosition());
        }

        public bool IsTrainPartiallyOnBlock(Train.Train train)
        {
            if (ContainsPosition(train.Position))
                return true;

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                if (ContainsPosition(train.GetVehicleTransform(i).Position))
                    return true;
            }

            return ContainsPosition(train.GetLastVehiclePosition());
        }

        public bool TryReserve(Train.Train train)
        {
            if (IsOccupied)
                return false;
            if (IsReserved && ReservedFor != train)
                return false;

            IsReserved = true;
            ReservedFor = train;
            return true;
        }

        public void ReleaseReservation()
        {
            IsReserved = false;
            ReservedFor = null;
        }

        public void Release(Train.Train train)
        {
            if (!ContainsTrain(train))
                return;

            RemoveTrain(train);
            if (_trainsOnBlock.Count == 0)
                ReleaseReservation();
        }

        public float GetDistanceFromStart(Vector2 position)
        {
            if (TrackCells.Count == 0)
                return 0f;

            float minDistance = float.MaxValue;
            float bestProgress = 0f;
            for (int i = 0; i < TrackCells.Count; i++)
            {
                var cellPos = new Vector2(TrackCells[i].X + 0.5f, TrackCells[i].Y + 0.5f);
                float dist = Vector2.Distance(position, cellPos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestProgress = i + 0.5f;
                }
            }
            return bestProgress;
        }

        public float GetDistanceToEnd(Vector2 position) => Length - GetDistanceFromStart(position);

        public Vector2 GetPositionAtProgress(float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            if (TrackCells.Count == 0)
                return Vector2.Zero;

            float exactIndex = progress * TrackCells.Count;
            int index = Math.Clamp((int)Math.Floor(exactIndex), 0, TrackCells.Count - 1);
            float fraction = exactIndex - index;
            var cellPos = new Vector2(TrackCells[index].X + 0.5f, TrackCells[index].Y + 0.5f);

            if (index >= TrackCells.Count - 1)
                return cellPos;

            var nextPos = new Vector2(TrackCells[index + 1].X + 0.5f, TrackCells[index + 1].Y + 0.5f);
            return Vector2.Lerp(cellPos, nextPos, fraction);
        }

        public void SetLength(float length) => Length = length;

        public override string ToString() => $"[Block {Id.ToString()[..8]}] Cells: {TrackCells.Count}, Length: {Length:F2}, Occupied: {IsOccupied}";
    }
}
