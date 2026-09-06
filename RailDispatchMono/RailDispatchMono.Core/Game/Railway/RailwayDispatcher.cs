using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>First-come-first-served block arbitration. It never moves switches or changes signal aspects.</summary>
public sealed class RailwayDispatcher
{
    private readonly BlockController _blocks;
    private readonly Dictionary<Guid, Block> _reservations = new();
    private readonly List<Guid> _pendingRequests = new();
    public static RailwayDispatcher? Current { get; private set; }

    public IReadOnlyList<Guid> PendingRequestTrainIds => _pendingRequests;
    public int PendingRequestCount => _pendingRequests.Count;

    public RailwayDispatcher(BlockController blocks)
    {
        _blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        Current = this;
    }

    public static bool TryCanProceed(Train.Train train)
    {
        var blocks = TrainManager.Current?.BlockController;
        if (blocks == null) return true;
        Current ??= new RailwayDispatcher(blocks);
        return Current.CanProceed(train);
    }

    public static void NotifyUpdated(Train.Train train) => Current?.Update(train);
    public static void NotifyReleased(Train.Train train) => Current?.Release(train);

    public bool CanProceed(Train.Train train)
    {
        if (train == null) return false;
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current == null) return true;
        var next = current.NextBlock;
        if (next == null || next == current) return true;

        bool blocked = next.IsOccupied && !next.ContainsTrain(train) ||
                       next.IsReserved && next.ReservedFor != train ||
                       next.IsCoolingDown;
        if (blocked)
        {
            Enqueue(train.Id);
            return false;
        }

        if (_pendingRequests.Count > 0 && _pendingRequests[0] != train.Id)
        {
            Enqueue(train.Id);
            return false;
        }

        if (!next.TryReserve(train))
        {
            Enqueue(train.Id);
            return false;
        }

        _reservations[train.Id] = next;
        RemoveRequest(train.Id);
        return true;
    }

    private void Enqueue(Guid trainId)
    {
        if (!_pendingRequests.Contains(trainId)) _pendingRequests.Add(trainId);
    }

    private void RemoveRequest(Guid trainId) => _pendingRequests.Remove(trainId);

    public void Update(Train.Train train)
    {
        if (train == null) return;
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current != null && _reservations.TryGetValue(train.Id, out var reserved) && current == reserved)
        {
            _reservations.Remove(train.Id);
            reserved.ReleaseReservation();
        }
        PruneRequests();
    }

    public void Release(Train.Train train)
    {
        if (train == null) return;
        if (_reservations.Remove(train.Id, out var block)) block.ReleaseReservation();
        RemoveRequest(train.Id);
        PruneRequests();
    }

    private void PruneRequests()
    {
        var manager = TrainManager.Current;
        if (manager == null) return;
        _pendingRequests.RemoveAll(id => !manager.Trains.Any(t => t.Id == id));
    }

    public string GetStatus(Train.Train train)
    {
        if (train == null) return "BRAK POCIĄGU";
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current == null) return "POZA BLOKAMI";
        var next = current.NextBlock;
        if (next == null) return "BRAK NASTĘPNEGO BLOKU";
        if (next.IsOccupied && !next.ContainsTrain(train)) return "BLOK ZAJĘTY — OCZEKIWANIE";
        if (next.IsReserved && next.ReservedFor != train) return "BLOK ZAREZERWOWANY — OCZEKIWANIE";
        if (next.IsCoolingDown) return "BLOK WYGASZANY — OCZEKIWANIE";
        if (_pendingRequests.Count > 0 && _pendingRequests[0] != train.Id && _pendingRequests.Contains(train.Id)) return "KOLEJKA DISPATCHERA";
        return _reservations.ContainsKey(train.Id) ? "DROGA ZAREZERWOWANA" : "DROGA DOSTĘPNA";
    }
}
