using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>First-come-first-served block arbitration. It never moves switches or changes signal aspects.</summary>
public sealed class RailwayDispatcher
{
    public readonly record struct PendingRequest(Guid TrainId, Guid BlockId);

    private readonly BlockController _blocks;
    private readonly Dictionary<Guid, Block> _reservations = new();
    private readonly List<PendingRequest> _pendingRequests = new();
    public static RailwayDispatcher? Current { get; private set; }

    public IReadOnlyList<Guid> PendingRequestTrainIds => _pendingRequests.Select(x => x.TrainId).ToList();
    public IReadOnlyList<PendingRequest> PendingRequests => _pendingRequests;
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
            Enqueue(train.Id, next.Id);
            return false;
        }

        var earlierRequest = _pendingRequests.FirstOrDefault(r => r.BlockId == next.Id);
        if (earlierRequest != default && earlierRequest.TrainId != train.Id)
        {
            Enqueue(train.Id, next.Id);
            return false;
        }

        if (!next.TryReserve(train))
        {
            Enqueue(train.Id, next.Id);
            return false;
        }

        _reservations[train.Id] = next;
        RemoveRequest(train.Id, next.Id);
        return true;
    }

    private void Enqueue(Guid trainId, Guid blockId)
    {
        if (!_pendingRequests.Any(r => r.TrainId == trainId && r.BlockId == blockId))
            _pendingRequests.Add(new PendingRequest(trainId, blockId));
    }

    private void RemoveRequest(Guid trainId, Guid blockId) => _pendingRequests.RemoveAll(r => r.TrainId == trainId && r.BlockId == blockId);

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
        _pendingRequests.RemoveAll(r => r.TrainId == train.Id);
        PruneRequests();
    }

    private void PruneRequests()
    {
        var manager = TrainManager.Current;
        if (manager == null) return;
        _pendingRequests.RemoveAll(request => !manager.Trains.Any(t => t.Id == request.TrainId));
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
        if (_pendingRequests.Any(r => r.TrainId == train.Id && r.BlockId == next.Id)) return "KOLEJKA DISPATCHERA";
        return _reservations.ContainsKey(train.Id) ? "DROGA ZAREZERWOWANA" : "DROGA DOSTĘPNA";
    }
}
