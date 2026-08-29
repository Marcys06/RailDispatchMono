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
    /// Blok torowy - odcinek toru między dwoma semaforami
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

        /// <summary>Komórki toru w bloku (kolejność = trasa)</summary>
        public List<MapPosition> TrackCells { get; }

        /// <summary>Punkty węzłowe dla ruchu pociągu</summary>
        public List<Vector2> Waypoints { get; }

        // ============================================================
        // SEMAFORY
        // ============================================================

        public Signal? EntrySignal { get; set; }
        public Signal? ExitSignal { get; set; }

        // ============================================================
        // STAN - POCIĄGI NA BLOKU
        // ============================================================

        /// <summary>Lista pociągów na bloku (obsługa długich pociągów)</summary>
        private readonly List<Train.Train> _trainsOnBlock = new();
        public IReadOnlyList<Train.Train> TrainsOnBlock => _trainsOnBlock;

        public bool IsOccupied => _trainsOnBlock.Count > 0;
        public Train.Train? OccupyingTrain => _trainsOnBlock.FirstOrDefault();

        // ============================================================
        // STAN - REZERWACJA
        // ============================================================

        public bool IsReserved { get; private set; }
        public Train.Train? ReservedFor { get; private set; }

        // ============================================================
        // WŁAŚCIWOŚCI
        // ============================================================

        public float Length { get; private set; }
        public float LengthInMeters => Length * 100f; // 1 jednostka = 100m

        // ============================================================
        // SĄSIEDZTWO
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
        }

        // ============================================================
        // METODY - POZYCJE
        // ============================================================

        public bool ContainsPosition(Vector2 position)
        {
            // Sprawdź czy pozycja znajduje się w którymkolwiek polu bloku
            foreach (var cell in TrackCells)
            {
                var cellPos = new Vector2(cell.X + 0.5f, cell.Y + 0.5f);
                if (Vector2.Distance(position, cellPos) < 0.6f)
                    return true;
            }
            return false;
        }

        public bool ContainsCell(MapPosition cell)
        {
            return TrackCells.Contains(cell);
        }

        // ============================================================
        // METODY - POCIĄGI NA BLOKU
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
        // METODY - SPRAWDZANIE POCIĄGU
        // ============================================================

        /// <summary>
        /// Sprawdza czy cały pociąg (od głowy do ogona) jest na bloku
        /// </summary>
        public bool IsTrainFullyOnBlock(Train.Train train)
        {
            var headPosition = train.Position;
            var tailPosition = train.GetLastVehiclePosition();

            return ContainsPosition(headPosition) && ContainsPosition(tailPosition);
        }

        /// <summary>
        /// Sprawdza czy jakakolwiek część pociągu jest na bloku
        /// </summary>
        public bool IsTrainPartiallyOnBlock(Train.Train train)
        {
            // Sprawdź głowę
            if (ContainsPosition(train.Position))
                return true;

            // Sprawdź wszystkie wagony
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var transform = train.GetVehicleTransform(i);
                if (ContainsPosition(transform.Position))
                    return true;
            }

            // Sprawdź ogon
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
        // METODY - OBLICZANIE ODLEGŁOŚCI
        // ============================================================

        public float GetDistanceFromStart(Vector2 position)
        {
            if (TrackCells.Count == 0)
                return 0f;

            // Znajdź najbliższy punkt na trasie
            float minDistance = float.MaxValue;
            float bestProgress = 0f;

            for (int i = 0; i < TrackCells.Count; i++)
            {
                var cellPos = new Vector2(TrackCells[i].X + 0.5f, TrackCells[i].Y + 0.5f);
                float dist = Vector2.Distance(position, cellPos);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestProgress = i + 0.5f; // Środek komórki
                }
            }

            // Przelicz na odległość
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
        // METODY - USTAWIENIE DŁUGOŚCI
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