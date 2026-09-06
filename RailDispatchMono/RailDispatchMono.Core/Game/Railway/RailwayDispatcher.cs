using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>First-come-first-served block arbitration over the existing block chain. It never moves switches or changes signal aspects.</summary>
public sealed class RailwayDispatcher
{
    public readonly record struct PendingRequest(Guid TrainId, Guid BlockId, DateTime RequestedAtUtc);

    private readonly BlockController _blocks;
    private readonly Dictionary<Guid, Block> _reservations = new();
    private readonly List<PendingRequest> _pendingRequests = new();
    private readonly HashSet<Guid> _forcedTrains = new();
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

    /// <summary>F6 dispatcher override. It bypasses dispatcher arbitration only; it never changes physical block occupancy.</summary>
    public static void ForceProceed(Train.Train train)
    {
        if (train == null || Current == null) return;
        Current._forcedTrains.Add(train.Id);
        Current._pendingRequests.RemoveAll(r => r.TrainId == train.Id);
    }

    public static void ClearForceProceed(Train.Train train) => Current?._forcedTrains.Remove(train.Id);
    public static bool IsForced(Train.Train train) => Current?._forcedTrains.Contains(train.Id) == true;

    public static double GetWaitSeconds(Train.Train train)
    {
        if (Current == null) return 0;
        var request = Current._pendingRequests.FirstOrDefault(r => r.TrainId == train.Id);
        return request == default ? 0 : Math.Max(0, (DateTime.UtcNow - request.RequestedAtUtc).TotalSeconds);
    }

    public static string GetRequestedBlockName(Train.Train train)
    {
        if (Current == null) return "—";
        var request = Current._pendingRequests.FirstOrDefault(r => r.TrainId == train.Id);
        if (request == default) return "—";
        return Current._blocks.Blocks.FirstOrDefault(b => b.Id == request.BlockId)?.Name ?? request.BlockId.ToString()[..8];
    }

    public bool CanProceed(Train.Train train)
    {
        if (train == null) return false;
        if (_forcedTrains.Contains(train.Id))
        {
            _pendingRequests.RemoveAll(r => r.TrainId == train.Id);
            return true;
        }

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

        // FCFS is local to the requested block. A train waiting for another block does not block this one.
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
            _pendingRequests.Add(new PendingRequest(trainId, blockId, DateTime.UtcNow));
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
        _forcedTrains.Remove(train.Id);
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
        if (_forcedTrains.Contains(train.Id)) return "OVERRIDE F6 — PRZEJAZD WYMUSZONY";
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current == null) return "POZA BLOKAMI";
        var next = current.NextBlock;
        if (next == null) return "BRAK NASTĘPNEGO BLOKU";
        if (next.IsOccupied && !next.ContainsTrain(train)) return "BLOKADA — OCZEKIWANIE";
        if (next.IsReserved && next.ReservedFor != train) return "TRASA ZAJĘTA — OCZEKIWANIE";
        if (next.IsCoolingDown) return "BLOK WYGASZANY — OCZEKIWANIE";
        if (_pendingRequests.Any(r => r.TrainId == train.Id && r.BlockId == next.Id)) return "KOLEJKA FCFS";
        return _reservations.ContainsKey(train.Id) ? "TRASA PRZYZNANA" : "TRASA DOSTĘPNA";
    }
}
