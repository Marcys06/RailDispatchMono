using RailDispatchMono.Core.Game.Train;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.RollingStock;

public static class RollingStockCatalog
{
    public static IReadOnlyList<LocomotiveDefinition> Locomotives { get; } = new[]
    {
        // 0.1.4e gameplay scaling: the existing EU06-like model is the baseline.
        new LocomotiveDefinition("EP07", "EP07", TractionType.Electric, LocomotiveType.ElectricDC,
            125f, 80f, 16.2f, 0.55f, 0.90f),
        // Newag Griffin E4ACP / EU200 passenger configuration.
        new LocomotiveDefinition("EU200", "EU200 — Newag Griffin E4ACP", TractionType.Electric, LocomotiveType.ElectricAC,
            200f, 84f, 19.9f, 0.75f, 1.00f),
        new LocomotiveDefinition("SU42", "SU42", TractionType.Diesel, LocomotiveType.Diesel,
            90f, 74f, 14.4f, 0.40f, 0.80f)
    };

    public static IReadOnlyList<WagonDefinition> Wagons { get; } = new[]
    {
        new WagonDefinition("PassengerCoach", "Wagon pasażerski", 40f, 26.4f, 160f, WagonType.Passenger, 80),
        new WagonDefinition("PassengerCoach2", "Wagon pasażerski 2", 38f, 26.4f, 200f, WagonType.Passenger, 80),
        new WagonDefinition("PassengerCoach3", "Wagon pasażerski 3", 42f, 26.4f, 160f, WagonType.Passenger, 96)
    };

    public static LocomotiveDefinition GetLocomotive(string id) =>
        Locomotives.First(x => x.Id == id);

    public static WagonDefinition GetWagon(string id) =>
        Wagons.First(x => x.Id == id);
}
