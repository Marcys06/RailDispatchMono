using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Tests;

public sealed class CouplingModelTests
{
    [Fact]
    public void DefaultCoupling_UsesScrewCouplersOnBothEnds()
    {
        var specification = CouplingSpecification.Default;

        Assert.Equal(CouplerType.Screw, specification.Front);
        Assert.Equal(CouplerType.Screw, specification.Rear);
        Assert.Equal(CouplerType.Screw, specification.Get(VehicleEnd.Front));
        Assert.Equal(CouplerType.Screw, specification.Get(VehicleEnd.Rear));
    }

    [Fact]
    public void CouplingConnection_MatchesBothDirections()
    {
        var first = CreateLocomotive("A");
        var second = CreateLocomotive("B");
        var connection = new CouplingConnection(first, VehicleEnd.Rear, second, VehicleEnd.Front);

        Assert.True(connection.Contains(first));
        Assert.True(connection.Contains(second));
        Assert.True(connection.Matches(first, VehicleEnd.Rear, second, VehicleEnd.Front));
        Assert.True(connection.Matches(second, VehicleEnd.Front, first, VehicleEnd.Rear));
        Assert.False(connection.Matches(first, VehicleEnd.Front, second, VehicleEnd.Rear));
    }

    [Fact]
    public void CouplingConnection_RejectsSelfConnection()
    {
        var vehicle = CreateLocomotive("A");

        Assert.Throws<ArgumentException>(() =>
            new CouplingConnection(vehicle, VehicleEnd.Rear, vehicle, VehicleEnd.Front));
    }

    [Fact]
    public void CouplingOperationResult_SeparatesSuccessFromFailureReason()
    {
        var success = CouplingOperationResult.Ok;
        var failure = CouplingOperationResult.Fail(CouplingFailureReason.TooFarApart);

        Assert.True(success.Success);
        Assert.Equal(CouplingFailureReason.None, success.Reason);
        Assert.False(failure.Success);
        Assert.Equal(CouplingFailureReason.TooFarApart, failure.Reason);
    }

    private static Locomotive CreateLocomotive(string shortName) =>
        new(
            LocomotiveType.ElectricDC,
            VehicleParameters.CreatePhysical(100f, 0.5f, 0.6f, 80f, 18f),
            shortName);
}
