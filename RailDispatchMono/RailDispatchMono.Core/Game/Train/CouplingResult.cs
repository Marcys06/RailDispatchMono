namespace RailDispatchMono.Core.Game.Train;

public enum CouplingFailureReason
{
    None,
    SameVehicle,
    SameTrain,
    EndOccupied,
    UnsupportedCoupler,
    TooFarApart,
    Misaligned,
    NotTrainBoundary,
    NotCoupled
}

public readonly record struct CouplingCheckResult(bool Allowed, CouplingFailureReason Reason)
{
    public static CouplingCheckResult Success => new(true, CouplingFailureReason.None);
    public static CouplingCheckResult Fail(CouplingFailureReason reason) => new(false, reason);
}

public readonly record struct CouplingOperationResult(bool Success, CouplingFailureReason Reason)
{
    public static CouplingOperationResult Ok => new(true, CouplingFailureReason.None);
    public static CouplingOperationResult Fail(CouplingFailureReason reason) => new(false, reason);
}
