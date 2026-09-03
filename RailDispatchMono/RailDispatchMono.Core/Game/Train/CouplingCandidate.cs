using Microsoft.Xna.Framework;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// Snapshot of a possible physical coupling between two train boundary ends.
/// It is intentionally UI-neutral and contains the result of the authoritative coupling validation.
/// </summary>
public readonly record struct CouplingCandidate(
    Train FirstTrain,
    int FirstVehicleIndex,
    VehicleEnd FirstEnd,
    Train SecondTrain,
    int SecondVehicleIndex,
    VehicleEnd SecondEnd,
    Vector2 FirstPoint,
    Vector2 SecondPoint,
    float Distance,
    CouplingCheckResult Check)
{
    public bool Allowed => Check.Allowed;
    public CouplingFailureReason Reason => Check.Reason;
}
