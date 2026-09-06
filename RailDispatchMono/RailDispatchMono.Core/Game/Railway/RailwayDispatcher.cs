using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>First-come-first-served block arbitration. It never moves switches or changes signal aspects.</summary>
public sealed class RailwayDispatcher
{
    private readonly BlockController _blocks;
    private readonly Dictionary<Guid, Block> _reservations = new();
    public static RailwayDispatcher? Current { get; private set; }

    public RailwayDispatcher(BlockController blocks)
    {
        _blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        Current = this;
    }

    public bool CanProceed(Train.Train train)
    {
        if (train == null) return false;
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current == null) return true;
        var next = current.NextBlock;
        if (next == null || next == current) return true;
        if (next.IsOccupied && !next.ContainsTrain(train)) return false;
        if (next.IsReserved && next.ReservedFor != train) return false;
        if (!next.TryReserve(train)) return false;
        _reservations[train.Id] = next;
        return true;
    }

    public void Update(Train.Train train)
    {
        if (train == null) return;
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current != null && _reservations.TryGetValue(train.Id, out var reserved) && current == reserved)
        {
            _reservations.Remove(train.Id);
            reserved.ReleaseReservation();
        }
    }

    public void Release(Train.Train train)
    {
        if (train == null) return;
        if (_reservations.Remove(train.Id, out var block)) block.ReleaseReservation();
    }

    public string GetStatus(Train.Train train)
    {
        if (train == null) return "BRAK POCIĄGU";
        var current = _blocks.GetBlockAtPosition(train.Position);
        if (current == null) return "POZA BLOKAMI";
        var next = current.NextBlock;
        if (next == null) return "BRAK NASTĘPNEGO BLOKU";
        if (next.IsOccupied && !next.ContainsTrain(train)) return "BLOK ZAJĘTY";
        if (next.IsReserved && next.ReservedFor != train) return "BLOK ZAREZERWOWANY";
        return _reservations.ContainsKey(train.Id) ? "DROGA ZAREZERWOWANA" : "DROGA DOSTĘPNA";
    }
}