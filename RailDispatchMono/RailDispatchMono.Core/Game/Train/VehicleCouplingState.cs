using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Runtime coupling state owned by a vehicle.</summary>
public sealed class VehicleCouplingState
{
    private CouplingConnection? _front;
    private CouplingConnection? _rear;

    public CouplingConnection? Front => _front;
    public CouplingConnection? Rear => _rear;

    internal CouplingConnection? Get(VehicleEnd end) => end == VehicleEnd.Front ? _front : _rear;

    internal bool IsOccupied(VehicleEnd end) => Get(end) != null;

    internal void Set(VehicleEnd end, CouplingConnection? connection)
    {
        if (end == VehicleEnd.Front)
            _front = connection;
        else
            _rear = connection;
    }

    public IEnumerable<CouplingConnection> Connections()
    {
        if (_front != null) yield return _front;
        if (_rear != null && !ReferenceEquals(_rear, _front)) yield return _rear;
    }
}
