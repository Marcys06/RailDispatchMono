using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Tests;

public sealed class TrainCompositionCouplingTests
{
    [Fact]
    public void Composition_Split_PreservesVehicleOrder()
    {
        var first = CreateLocomotive("EP07");
        var second = CreateLocomotive("1KL-01");
        var third = CreateLocomotive("1KL-02");
        var fourth = CreateLocomotive("1KL-03");
        var composition = new TrainComposition(new[] { first, second, third, fourth });

        var split = composition.Split(2);

        Assert.Equal(new[] { first, second }, composition.Vehicles);
        Assert.Equal(new[] { third, fourth }, split.Vehicles);
    }

    [Fact]
    public void Composition_Split_AtZero_MovesWholeConsist()
    {
        var first = CreateLocomotive("A");
        var second = CreateLocomotive("B");
        var composition = new TrainComposition(new[] { first, second });

        var split = composition.Split(0);

        Assert.Empty(composition.Vehicles);
        Assert.Equal(new[] { first, second }, split.Vehicles);
    }

    [Fact]
    public void Train_ConstructsCompositionInExactInputOrder()
    {
        var first = CreateLocomotive("A");
        var second = CreateLocomotive("B");
        var third = CreateLocomotive("C");

        var train = new Train(
            Vector2.Zero,
            TrackConnections.East,
            0f,
            new[] { first, second, third });

        Assert.Equal(new[] { first, second, third }, train.Composition.Vehicles);
    }

    private static Locomotive CreateLocomotive(string shortName) =>
        new(
            LocomotiveType.ElectricDC,
            VehicleParameters.CreatePhysical(100f, 0.5f, 0.6f, 80f, 18f),
            shortName);
}
