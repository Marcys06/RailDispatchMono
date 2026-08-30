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
    /// Blok torowy - odcinek toru miêdzy dwoma semaforami
    /// </summary>
    public class Block
    {
        // ============================================================
        // IDENTYFIKACJA
        // ============================================================

        public Guid Id { get; }
        public string Name { get; set; }

        // ============================================================
        // DANE PRZESTRZENNE
        // ============================================================

        /// <summary>Komórki toru w bloku (kolejnoœæ = trasa)</summary>
        public List<MapPosition> TrackCells { get; }

        /// <summary>Punkty wêz³owe dla ruchu poci¹gu</summary>
        public List<Vector2> Waypoints { get; }

        // ============================================================
        // SEMAFORY
        // ============================================================

        public Signal? EntrySignal { get; set; }
        public Signal? ExitSignal { get; set; }

        // ============================================================
        // STAN - POCI¥GI NA BLOKU
        // ============================================================

        /// <summary>Lista poci¹gów na bloku (obs³uga d³ugich poci¹gów)</summary>
        private readonly List<Train.Train> _trainsOnBlock = new();
        public IReadOnlyList<Train.Train> TrainsOnBlock => _trainsOnBlock;

        public bool IsOccupied => _trainsOnBlock.Count > 0;
        public Train.Train? OccupyingTrain => _trainsOnBlock.FirstOrDefault();

        // ============================================================
        // STAN - COOLDOWN SZYBKIEGO PRZE£¥CZANIA
        // ============================================================

        private float _coolDownTimer = 0f;
        private bool _isCoolingDown = false;
        private const float CoolDownDuration = 0.5f; // 0.5 sekundy

        public bool IsCoolingDown => _isCoolingDown;
        public bool IsOccupiedOrCoolingDown => IsOccupied || IsCoolingDown;

        // ============================================================
        // STAN - REZERWACJA
        // ============================================================

        public bool IsReserved { get; private set; }
        public Train.Train? ReservedFor { get; private set; }

        // ============================================================
        // W£AŒCIWOŒCI
        // ============================================================

        public float Length { get; private set; }
        public float LengthInMeters => Length * 100f; // 1 jednostka = 100m

        // ============================================================
        // S¥SIEDZTWO
        // ============================================================

        public Block? PreviousBlock { get; set; }
        public Block? NextBlock { get; set; }
        public List<Block> AlternativeNextBlocks { get; }

        // ============================================================
        // KONSTRUKTOR
        // ============================================================

        public Block()
        {
            Id = Guid.NewGuid();
            Name = $"Block_{Id.ToString()[..8]}";
            TrackCells = new List<MapPosition>();
            Waypoints = new List<Vector2>();
            AlternativeNextBlocks = new List<Block>();
            Length = 0f;
            IsReserved = false;
            ReservedFor = null;
            _coolDownTimer = 0f;
            _isCoolingDown = false;
        }

        // ============================================================
        // METODY - POZYCJE
        // ============================================================

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

        public bool ContainsCell(MapPosition cell)
        {
            return TrackCells.Contains(cell);
        }

        // ============================================================
        // METODY - POCI¥GI NA BLOKU
        // ============================================================

        public void AddTrain(Train.Train train)
        {
            if (!_trainsOnBlock.Contains(train))
            {
                _trainsOnBlock.Add(train);
            }
        }

        public void RemoveTrain(Train.Train train)
        {
            _trainsOnBlock.Remove(train);
        }

        public bool ContainsTrain(Train.Train train)
        {
            return _trainsOnBlock.Contains(train);
        }

        public void ClearTrains()
        {
            _trainsOnBlock.Clear();
        }

        // ============================================================
        // METODY - COOLDOWN
        // ============================================================

        public void StartCooldown()
        {
            _coolDownTimer = CoolDownDuration;
            _isCoolingDown = true;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (_isCoolingDown)
            {
                _coolDownTimer -= deltaTime;
                if (_coolDownTimer <= 0f)
                {
                    _isCoolingDown = false;
                    _coolDownTimer = 0f;
                }
            }
        }

        // ============================================================
        // METODY - WYJŒCIE POCI¥GU Z BLOKU
        // ============================================================

        public void OnTrainExited(Train.Train train)
        {
            // Usuñ poci¹g z bloku
            RemoveTrain(train);

            // Jeœli blok jest pusty, rozpocznij cooldown
            if (!IsOccupied)
            {
                StartCooldown();
                // Semafor zostanie zresetowany w UpdateSignals() po cooldownie
            }
        }

        // ============================================================
        // METODY - RESETOWANIE SEMAFORÓW
        // ============================================================

        public void ResetEntrySignals()
        {
            if (EntrySignal != null)
            {
                EntrySignal.SetAspect(SignalAspect.Clear);
                DebugManager.Log($"[BLOCK] {Name} › RESET entry signal to Clear");
            }
        }

        // ============================================================
        // METODY - SPRAWDZANIE POCI¥GU
        // ============================================================

        /// <summary>
        /// Sprawdza czy ca³y poci¹g (od g³owy do ogona) jest na bloku
        /// </summary>
        public bool IsTrainFullyOnBlock(Train.Train train)
        {
            var headPosition = train.Position;
            var tailPosition = train.GetLastVehiclePosition();

            return ContainsPosition(headPosition) && ContainsPosition(tailPosition);
        }

        /// <summary>
        /// Sprawdza czy jakakolwiek czêœæ poci¹gu jest na bloku
        /// </summary>
        public bool IsTrainPartiallyOnBlock(Train.Train train)
        {
            if (ContainsPosition(train.Position))
                return true;

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var transform = train.GetVehicleTransform(i);
                if (ContainsPosition(transform.Position))
                    return true;
            }

            if (ContainsPosition(train.GetLastVehiclePosition()))
                return true;

            return false;
        }

        // ============================================================
        // METODY - REZERWACJA
        // ============================================================

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
            if (ContainsTrain(train))
            {
                RemoveTrain(train);
                if (_trainsOnBlock.Count == 0)
                {
                    ReleaseReservation();
                }
            }
        }

        // ============================================================
        // METODY - OBLICZANIE ODLEG£OŒCI
        // ============================================================

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

            float cellLength = 1.0f;
            return bestProgress * cellLength;
        }

        public float GetDistanceToEnd(Vector2 position)
        {
            return Length - GetDistanceFromStart(position);
        }

        // ============================================================
        // METODY - POZYCJA NA TRASIE
        // ============================================================

        public Vector2 GetPositionAtProgress(float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);

            if (TrackCells.Count == 0)
                return Vector2.Zero;

            float totalCells = TrackCells.Count;
            float exactIndex = progress * totalCells;
            int index = (int)Math.Floor(exactIndex);
            float fraction = exactIndex - index;

            index = Math.Clamp(index, 0, TrackCells.Count - 1);

            var cell = TrackCells[index];
            Vector2 cellPos = new Vector2(cell.X + 0.5f, cell.Y + 0.5f);

            if (index >= TrackCells.Count - 1)
                return cellPos;

            var nextCell = TrackCells[index + 1];
            Vector2 nextPos = new Vector2(nextCell.X + 0.5f, nextCell.Y + 0.5f);

            return Vector2.Lerp(cellPos, nextPos, fraction);
        }

        // ============================================================
        // METODY - USTAWIENIE D£UGOŒCI
        // ============================================================

        public void SetLength(float length)
        {
            Length = length;
        }

        // ============================================================
        // METODY - TO STRING
        // ============================================================

        public override string ToString()
        {
            return $"[Block {Id.ToString()[..8]}] Cells: {TrackCells.Count}, Length: {Length:F2}, Occupied: {IsOccupied}";
        }
    }
}